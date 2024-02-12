#define usetasks

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UeiBridge;
using UeiBridge.Library.CubeSetupTypes;
using UeiBridge.Library.Interfaces;
using UeiBridge.Library;
//using UeiBridge.Library.Types;
using UeiDaq;
using System.Collections.Concurrent;

namespace UeiBridge
{
    /// <summary>
    /// Manage serial device.
    /// - Handle upstream/downstream messages 
    /// - sends updates to given watchdog
    /// - maintain channel statistics
    /// </summary>
    public class SL508DeviceManager : IDeviceManager, Library.Interfaces.IEnqueue<byte[]>
    {
        // publics
        public List<ChannelStat> ChannelStatList { get; private set; } // note that the index of this list is NOT (necessarily) the channel index
        public string DeviceName { get; } = DeviceMap2.SL508Literal;
        public string InstanceName { get; protected set; } = "SL508DeviceManager/{instanceName}";

        // privates
        const int _maxReadMesageLength = 400;
        IEnqueue<SendObject2> _readMessageConsumer;
        SL508892Setup _deviceSetup;
        Session _serialSession;
        List<ChannelAux2> _channelAuxList; // note that the index of this list is NOT (necessarily) the channel index
        IWatchdog _watchdog;
        //readonly log4net.ILog _logger = StaticLocalMethods.GetLogger();
        readonly log4net.ILog _logger = log4net.LogManager.GetLogger("SL508Manager");
        BlockingCollection<EthernetMessage> _downstreamQueue = new BlockingCollection<EthernetMessage>(100); // max 100 items
        Action<string> _onErrorCallback = new Action<string>(s => Console.WriteLine($"Failed to parse downstream message. {s}"));
        List<ViewItem<byte[]>> _lastScanList;// = new List<ViewItem<byte[]>>();
        List<ViewItem<byte[]>> _ViewItemList;// = new List<ViewItem<byte[]>>();
        int _deviceSlotIndex;
        bool _isOutputDeviceReady = true;
        bool _inDisposeFlag = false;
        CancellationTokenSource _cancelTokenSource = new CancellationTokenSource(); // cancel downstream and upstream tasks
        Task _downstreamTask;

        public SL508DeviceManager() { } // default c-tor must exists
        public SL508DeviceManager(IEnqueue<SendObject2> readMessageConsumer, SL508892Setup setup, Session serialSession)
        {
            this._readMessageConsumer = readMessageConsumer;
            this._deviceSetup = setup;
            this._serialSession = serialSession;
            this._deviceSlotIndex = setup.SlotNumber;

            this.InstanceName = $"{setup.GetInstanceName()}";
        }
        /// <summary>
        /// Set a watchdog for this class.
        /// Note that this method should be called (if needed) before StartDevice
        /// </summary>
        public void SetWatchdog(IWatchdog wd)
        {
            _watchdog = wd;
        }
        /// <summary>
        /// Push message to downstream task. (single task for all downstream channels)
        /// Before pushing , the message is translated.
        /// </summary>
        public void Enqueue(byte[] message)
        {
            if ((_downstreamQueue.IsCompleted) || (_isOutputDeviceReady == false))
            {
                return;
            }

            try
            {
                EthernetMessage em = EthernetMessage.CreateFromByteArray(message, MessageWay.downstream, _onErrorCallback); // hmm.. not sure that this is the right place to translate
                if (null == em)
                {
                    return;
                }

                if (false == _downstreamQueue.TryAdd(em))
                {
                    Console.WriteLine($"Incoming message dropped due to full message queue");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Downstream message dropped. {ex.Message}.");
            }
        }
        void Task_DownstreamMessageLoop( SL508892Setup setup)
        {

            //_logger.Info($"{setup.DeviceName} Writer started");
            // message loop
            // ============
            while ((_cancelTokenSource.IsCancellationRequested == false) || (_downstreamQueue.IsCompleted == false))
            {
                try
                {
                    EthernetMessage incomingMessage = _downstreamQueue.Take();

                    // verify internal consistency
                    if (false == incomingMessage.InternalValidityTest())
                    {
                        Console.WriteLine("Invalid message. rejected");
                        continue;
                    }
                    // verify valid card type
                    int cardId = DeviceMap2.GetDeviceIdFromName(this.DeviceName);
                    if (cardId != incomingMessage.CardType)
                    {
                        Console.WriteLine($"{InstanceName} wrong card id {incomingMessage.CardType} while expecting {cardId}. message dropped.");
                        continue;
                    }
                    // verify slot number
                    if (incomingMessage.SlotNumber != this._deviceSlotIndex)
                    {
                        _logger.Info($"{InstanceName} wrong slot number ({incomingMessage.SlotNumber}). incoming message dropped.");
                        continue;
                    }
                    // alert if items lost
                    if (_downstreamQueue.Count == _downstreamQueue.BoundedCapacity)
                    {
                        _logger.Info($"Input queue items = {_downstreamQueue.Count}");
                    }

                    // finally, Handle message
                    if (_isOutputDeviceReady)
                    {
                        HandleDownstreamRequest(incomingMessage);
                    }
                    else
                    {
                        _logger.Info($"Device {DeviceName} not ready. message rejected.");
                    }
                }
                catch (InvalidOperationException ex) // thrown if _downstreamQueue marked as complete
                {
                    // nothing to do here
                }
                catch (Exception ex)
                {
                    _logger.Warn(ex.Message);
                }
            }
            _downstreamQueue.CompleteAdding(); // just mark queue as complete if task terminated from other reason
            _logger.Info("Task_DownstreamMessageLoop ended");
        }

        public void Dispose()
        {
            if (true == _inDisposeFlag)
            {
                return;
            }
            _inDisposeFlag = true;

            _watchdog?.Dispose();

            _cancelTokenSource.Cancel();
            _downstreamQueue.CompleteAdding();

            if (_downstreamTask.Status == TaskStatus.Running)
            {
                _downstreamTask.Wait();
            }
#if usetasks
            var runningTasks = _channelAuxList.Where(entry => entry.ReadTask.Status == TaskStatus.Running).Select(entry => entry.ReadTask);
            Task.WaitAll(runningTasks.ToArray());
#else
            var readersWaitHandle = _channelAuxList.Select(i => i.AsyncResult.AsyncWaitHandle).ToArray();
            WaitHandle.WaitAll(readersWaitHandle);
#endif
            foreach (var cx in _channelAuxList)
            {
                cx.Reader.Dispose();
                cx.Writer.Dispose();
            }

            _logger.Info("Readers/writers disposed..");
        }
        public bool StartDevice()
        {
            if (_inDisposeFlag)
            {
                return false;
            }
            _channelAuxList = new List<ChannelAux2>();
            ChannelStatList = new List<ChannelStat>();

            int numberOfChannels = _serialSession.GetNumberOfChannels();
            _lastScanList = new List<ViewItem<byte[]>>(new ViewItem<byte[]>[numberOfChannels]);
            _ViewItemList = new List<ViewItem<byte[]>>(new ViewItem<byte[]>[numberOfChannels]);

            // build serial readers and writers
            // --------------------------------
            for (int chNum = 0; chNum < _serialSession.GetNumberOfChannels(); chNum++)
            {
                // get channel index
                SerialPort sPort = _serialSession.GetChannel(chNum) as SerialPort;
                int chIndex = sPort.GetIndex();

                // create reader & writer 
                var reader = new SerialReader(_serialSession.GetDataStream(), chIndex);
                var writer = new SerialWriter(_serialSession.GetDataStream(), chIndex);

                // add channel-aux to list
                ChannelAux2 chAux = new ChannelAux2(chIndex, reader, writer, _serialSession);
                _channelAuxList.Add(chAux);
                ChannelStatList.Add(new ChannelStat(chIndex));
            }

            // Create reader tasks
            // --------------------
            
            foreach (ChannelAux2 cx in _channelAuxList)
            {
#if usetasks
                cx.ReadTask = Task.Factory.StartNew(() => Task_UpstreamMessageLoop(cx), _cancelTokenSource.Token);
#else
                cx.AsyncResult = cx.Reader.BeginRead(200, new AsyncCallback(ReaderCallback), cx); // start reading from device
#endif
            }
            
            // start downstream message loop
            _downstreamTask = Task.Run(() => Task_DownstreamMessageLoop(_deviceSetup), _cancelTokenSource.Token);

            return true;
        }
        void ReaderCallback(IAsyncResult ar)
        {
            if (true == _inDisposeFlag)
            {
                return;
            }
            ChannelAux2 chAux = ar.AsyncState as ChannelAux2;
            int chIndex = chAux.ChannelIndex;
            string chName = $"Com{chIndex}";
            try
            {
                byte[] recvBytes = chAux.Reader.EndRead(ar);

                System.Diagnostics.Debug.Assert(null != recvBytes);
                System.Diagnostics.Debug.Assert(null != chAux.OriginatingSession);
                System.Diagnostics.Debug.Assert(true == chAux.OriginatingSession.IsRunning());

                //EthernetMessage em = StaticMethods.BuildEthernetMessageFromDevice(recvBytes, this._thisDeviceSetup, chIndex);
                //_targetConsumer?.Send(new SendObject(  _thisDeviceSetup.DestEndPoint.ToIpEp(), em.GetByteArray(MessageWay.upstream)));
                _logger.Info($"Message from channel {chIndex}. Length {recvBytes.Length}");
                _watchdog?.NotifyAlive(chName);
                ChannelStat chStat = ChannelStatList.Where(i => i.ChannelIndex == chIndex).FirstOrDefault();
                chStat.ReadByteCount += recvBytes.Length;
                chStat.ReadMessageCount++;
                chAux.AsyncResult = chAux.Reader.BeginRead(200, new AsyncCallback(ReaderCallback), chAux);
            }
            catch (UeiDaqException ex)
            {
                // only handle exception if the session is running
                if (chAux.OriginatingSession.IsRunning())
                {
                    if (Error.Timeout == ex.Error)
                    {
                        //_logger.Info($"Timeout ch {chIndex}");
                        _watchdog.NotifyAlive(chName);
                        if (false == _inDisposeFlag)
                        {
                            chAux.AsyncResult = chAux.Reader.BeginRead(200, new AsyncCallback(ReaderCallback), chAux);
                        }
                    }
                    else
                    {
                        _logger.Info($"{chName} read error: {ex.Message}");
                        _watchdog.NotifyCrash(chName, ex.Message);
                    }
                }
                System.Diagnostics.Debug.Assert(true == chAux.OriginatingSession.IsRunning());
            }
        }

        /// <summary>
        /// Write Ethernet message to serial channel.
        /// and update stat counters
        /// </summary>
        protected void HandleDownstreamRequest(EthernetMessage request)
        {
            if (true == _inDisposeFlag)
            {
                return;
            }

            int chIndex = request.SerialChannelNumber;
            ChannelAux2 cx = _channelAuxList.Where(i => i.ChannelIndex == chIndex).FirstOrDefault();
            if (null == cx)
            {
                _logger.Info("Message from Ethernet with non exists channel index. rejected");
                return;
            }
            //UeiDaq.SerialWriter sw = cx.Writer;
            System.Diagnostics.Debug.Assert(cx.Writer != null);

            string chName = $"Com{cx.ChannelIndex}";
            
            try
            {
                // write to serial port
                int writtenBytes = cx.Writer.Write(request.PayloadBytes);
                if (writtenBytes != request.PayloadBytes.Length)
                {
                    _logger.Warn($"Failed in writing to serial channel {chName} ");
                }

                _ViewItemList[request.SerialChannelNumber] = new ViewItem<byte[]>(request.PayloadBytes, TimeSpan.FromSeconds(5));

                // wait state
                SerialPort sPort = _serialSession.GetChannel(chIndex) as SerialPort;
                int sp = StaticMethods.GetSerialSpeedAsInt(sPort.GetSpeed());
                double bpsPossibe = Convert.ToDouble(sp) * 0.8;
                int bitsToSend = request.PayloadBytes.Length * 8;
                double waitstateMs = Convert.ToDouble(bitsToSend) / bpsPossibe * 1000.0; // in mili-sec
                System.Threading.Thread.Sleep(Convert.ToInt32(waitstateMs));

                // update counters
                ChannelStat chStat = ChannelStatList.Where(i => i.ChannelIndex == chIndex).FirstOrDefault();
                chStat.WrittenByteCount += writtenBytes;
                chStat.WrittenMessageCount++;
            }
            catch (UeiDaqException ex)
            {
                _watchdog.NotifyCrash(chName, ex.Message);
                _logger.Warn($"UeiDaqException: {ex.Message}");
            }
            catch (Exception ex)
            {
                _watchdog.NotifyCrash(chName, ex.Message);
                _logger.Warn($"General exception: {ex.Message}");
            }
        }

        public string[] GetFormattedStatus(TimeSpan interval)
        {
            List<string> resultList = new List<string>();

            // upstream
            for (int ch = 0; ch < _lastScanList.Count; ch++)
            {
                var item = _lastScanList[ch];
                if (null != item)
                {
                    if (item.TimeToLive > TimeSpan.Zero)
                    {
                        item.DecreaseTimeToLive(interval);
                        int len = (item.ReadValue.Length > 20) ? 20 : item.ReadValue.Length;
                        string s = $"Ch{ch}: Upstream ({item.ReadValue.Length}): {BitConverter.ToString(item.ReadValue).Substring(0, len * 3 - 1)}";
                        resultList.Add(s);
                    }
                    else
                    {
                        item = null;
                    }
                }
            }

            // downstream
            for (int ch = 0; ch < _ViewItemList.Count; ch++)
            {
                var item = _ViewItemList[ch];
                if (null != item)
                {
                    if (item.TimeToLive > TimeSpan.Zero)
                    {
                        item.DecreaseTimeToLive(interval);
                        int len = (item.ReadValue.Length > 20) ? 20 : item.ReadValue.Length;
                        string s = $"Ch{ch}: Downstream ({item.ReadValue.Length}): {BitConverter.ToString(item.ReadValue).Substring(0, len * 3 - 1)}";
                        resultList.Add(s);
                    }
                    else
                    {
                        item = null;
                    }
                }
            }

            if (resultList.Count > 0)
            {
                return resultList.ToArray();
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Read message from serial device and send to target consumer, in a loop.
        /// This task is per single channel (uart)
        /// </summary>
        /// <param name="cx"></param>
        protected void Task_UpstreamMessageLoop(ChannelAux2 cx)
        {
            string chName = $"Com{cx.ChannelIndex}";
            var destEp = _deviceSetup.DestEndPoint.ToIpEp();

            // show establishment log messae
            {
                SerialPort serialCh = cx.OriginatingSession.GetChannel(cx.ChannelIndex) as SerialPort;
                UeiDevice ud = new UeiDevice(cx.OriginatingSession.GetDevice().GetResourceName());
                int speed = StaticMethods.GetSerialSpeedAsInt(serialCh.GetSpeed());
                _logger.Info($"Cube{ud.GetCubeId()}/{ud.LocalPath}/{chName} Reader ready. ({serialCh.GetMode()}/{speed}bps). Dest {destEp.ToString()}");
            }

            // register to watch dot
            _watchdog?.Register(chName, TimeSpan.FromSeconds(2.0)); // Hmm.. two second ... should use value relative to the value passed to .SetTimeout();

            // define message-builder delegate
            Func<byte[], byte[]> ethMsgBuilder = new Func<byte[], byte[]>( (buf) =>
            {
                EthernetMessage em1 = StaticMethods.BuildEthernetMessageFromDevice(buf, this._deviceSetup, cx.ChannelIndex);
                SerialChannelSetup chSetup = _deviceSetup.Channels[cx.ChannelIndex];
                if (true == chSetup.FilterByLength)
                {
                    if (buf.Length!=chSetup.MessageLength)
                    {
                        return null;
                    }
                }
                if (true == _deviceSetup.Channels[cx.ChannelIndex].FilterBySyncBytes)
                {
                    if ((buf[0]!=chSetup.SyncByte0)||(buf[1]!=chSetup.SyncByte1))
                    {
                        return null;
                    }
                }
                return em1.GetByteArray(MessageWay.upstream);
            }
            );
            
            try
            {
                //message loop
                do
                {
                    // wait for available messages
                    //-----------------------------
                    int available = 0;
                    do
                    {
                        // "available" is the amount of data remaining from a single event
                        // more data may be in the queue once "available" bytes have been read
                        available = cx.OriginatingSession.GetDataStream().GetAvailableInputMessages(cx.ChannelIndex);
                        _watchdog?.NotifyAlive(chName);
                        if (available == 0)
                        {
                            System.Threading.Thread.Sleep(5);
                        }
                    } while ((available == 0) && (false == _cancelTokenSource.IsCancellationRequested));

                    if (_cancelTokenSource.IsCancellationRequested)
                    {
                        continue; // this will break message loop
                    }

                    // get message from device and send to consumer
                    // --------------------------------------------
                    byte[] recvBytes = cx.Reader.Read(_maxReadMesageLength);
                    if (recvBytes.Length < _maxReadMesageLength)
                    {
                        _logger.Debug($"Message from channel {cx.ChannelIndex}. Length {recvBytes.Length}");
                    }
                    else
                    {
                        _logger.Warn($"Suspicious Message from channel {cx.ChannelIndex}. Length {recvBytes.Length}");
                    }
                    // send to consumer
                    _readMessageConsumer.Enqueue(new SendObject2(destEp, ethMsgBuilder, recvBytes));
                    // update status viewer
                    _lastScanList[cx.ChannelIndex] = new ViewItem<byte[]>(recvBytes, TimeSpan.FromSeconds(5));

                    // update stat
                    //ChannelStat chStat = ChannelStatList.Where(i => i.ChannelIndex == cx.ChannelIndex).FirstOrDefault();
                    //chStat.ReadByteCount += recvBytes.Length;
                    //chStat.ReadMessageCount++;

                } while (false == _cancelTokenSource.IsCancellationRequested);
            }
            catch (UeiDaqException e)
            {
                _logger.Info($"{chName} read exception. {e.Message}");
                _watchdog?.NotifyCrash(chName, e.Message);
            }
            _logger.Info($"Stopped reading {chName}");
        }
        public static Session BuildSerialSession2(SL508892Setup deviceSetup)
        {
            if (null == deviceSetup)
            {
                return null;
            }
            string deviceuri = $"{deviceSetup.CubeUrl}Dev{deviceSetup.SlotNumber}/";
            try
            {
                Session serialSession = new Session();

                UeiCube cube2 = new UeiCube(deviceSetup.CubeUrl);
                if (cube2.DeviceReset(deviceuri))
                {
                    foreach (var channelSetup in deviceSetup.Channels)
                    {
                        if (false == channelSetup.IsEnabled)
                        {
                            continue;
                        }
                        string finalUri = $"{deviceSetup.CubeUrl}Dev{deviceSetup.SlotNumber}/Com{channelSetup.ComIndex}";
                        SerialPort sport = serialSession.CreateSerialPort(finalUri,
                                            channelSetup.Mode,
                                            channelSetup.Baudrate,
                                            SerialPortDataBits.DataBits8,
                                            channelSetup.Parity,
                                            channelSetup.Stopbits,
                                            "");
                        System.Diagnostics.Debug.Assert(null != sport);
                    }

                    // just verify that there are N channels (serial  ports)
                    int chCount = deviceSetup.Channels.Where(ch => ch.IsEnabled == true).ToList().Count;
                    System.Diagnostics.Debug.Assert(serialSession.GetNumberOfChannels() == chCount);
                }
                else
                {
                    Console.WriteLine($"Failed to reset device {deviceuri}");
                    return null;
                }

                // Configure timing to return serial message when either of the following conditions occurred
                // - The termination string was detected
                // - 100 bytes have been received
                // - 10ms elapsed (rate set to 100Hz);
                serialSession.ConfigureTimingForMessagingIO(1000, 100.0);
                serialSession.GetTiming().SetTimeout(500);

                return serialSession;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating session. {ex.Message}");
                return null;
            }
        }

#if old
        public bool OpenDevice()
        {
            if (_inDisposeFlag)
            {
                return false;
            }
            _channelAuxList = new List<ChannelAux>();
            ChannelStatList = new List<ChannelStat>();

            // set serial channels and add them to channel list
            // ------------------------------------------------
            for (int chNum = 0; chNum < _serialSsession.GetNumberOfChannels(); chNum++)
            {
                // set channel properties
                SerialPort sPort = _serialSsession.GetChannel(chNum) as SerialPort;
                int chIndex = sPort.GetIndex();
                SerialChannelSetup channelSetup = _thisDeviceSetup.GetChannelEntry(chIndex);
                if (null != channelSetup)
                {
                    sPort.SetMode(channelSetup.Mode);
                    sPort.SetSpeed(channelSetup.Baudrate);
                    sPort.SetParity(channelSetup.Parity);
                    sPort.SetStopBits(channelSetup.Stopbits);
                    sPort.SetDataBits(SerialPortDataBits.DataBits8);
                }
                else
                {
                    Console.WriteLine($"Could not find setup for channel {chIndex}. Using defaults");
                }

                SerialPort sPort1 = _serialSsession.GetChannel(chNum) as SerialPort;
                Console.WriteLine($"Com {chIndex}: {sPort1.GetMode()} {sPort1.GetSpeed()}");

                // set reader & writer and add channel to channel-list
                var reader = new SerialReader(_serialSsession.GetDataStream(), chIndex);
                var writer = new SerialWriter(_serialSsession.GetDataStream(), chIndex);
                ChannelAux chAux = new ChannelAux(chIndex, reader, writer, _serialSsession);
                _channelAuxList.Add(chAux);
                ChannelStatList.Add(new ChannelStat(chIndex));

                // register to WD service
                _watchdog?.Register($"Com{chIndex}", TimeSpan.FromSeconds(2.0)); // Hmm.. two second ... should use value relative to the value passed to .SetTimeout();
            }

            // start readers
            foreach (ChannelAux cx in _channelAuxList)
            {
                cx.AsyncResult = cx.Reader.BeginRead(200, new AsyncCallback(ReaderCallback), cx); // start reading from device
            }


            _downstreamTask = Task.Factory.StartNew(DownstreamMessageLoop_Task, _cancelTokenSource.Token);

            return false;
        }
#endif

    }
}
