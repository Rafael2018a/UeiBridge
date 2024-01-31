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

namespace SerialOp
{
    /// <summary>
    /// Manage serial device.
    /// - Handle upstream/downstream messages 
    /// - update a given watchdog
    /// - maintain channel statistics
    /// </summary>
    public class SL508DeviceManager : IDeviceManager
    {
        private ISend<SendObject> _targetConsumer;
        SL508892Setup _thisDeviceSetup;
        Session _serialSsession;
        List<ChannelAux> _channelAuxList; // note that the index of this list is NOT (necessarily) the channel index
        public List<ChannelStat> ChannelStatList { get; private set; } // note that the index of this list is NOT (necessarily) the channel index
        IWatchdog _watchdog;
        uint linenumber = 0;
        //CancellationTokenSource _upstreamTaskCTS = new CancellationTokenSource();
        // publics
        public string DeviceName { get; protected set; }
        public string InstanceName { get; protected set; }
        // protected
        protected int _deviceSlotIndex;
        protected bool _isOutputDeviceReady = true;
        protected bool _inDisposeFlag = false;
        protected CancellationTokenSource _cancelTokenSource = new CancellationTokenSource();
        protected Task _downstreamTask;
        
        // privates
        private BlockingCollection<EthernetMessage> _downstreamQueue = new BlockingCollection<EthernetMessage>(100); // max 100 items
        private Action<string> _act = new Action<string>(s => Console.WriteLine($"Failed to parse downstream message. {s}"));

        /// <summary>
        /// Translate and enqueue downstream message
        /// </summary>
        public void Enqueue(byte[] message)
        {
            if ((_downstreamQueue.IsCompleted) || (_isOutputDeviceReady == false))
            {
                return;
            }

            try
            {
                EthernetMessage em = EthernetMessage.CreateFromByteArray(message, MessageWay.downstream, _act);
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
                Console.WriteLine($"Incoming message dropped. {ex.Message}.");
            }

        }

        protected void DownstreamMessageLoop_Task()
        {
            // message loop
            // ============
            while (_cancelTokenSource.IsCancellationRequested == false)
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
                        Console.WriteLine($"{InstanceName} wrong slot number ({incomingMessage.SlotNumber}). incoming message dropped.");
                        continue;
                    }
                    // alert if items lost
                    if (_downstreamQueue.Count == _downstreamQueue.BoundedCapacity)
                    {
                        Console.WriteLine($"Input queue items = {_downstreamQueue.Count}");
                    }

                    // finally, Handle message
                    if (_isOutputDeviceReady)
                    {
                        HandleDownstreamRequest(incomingMessage);
                    }
                    else
                    {
                        Console.WriteLine($"Device {DeviceName} not ready. message rejected.");
                    }
                }
                catch (InvalidOperationException ex) // _downstreamQueue marked as complete (for task termination)
                {
                    //Console.WriteLine(ex.Message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            _downstreamQueue.CompleteAdding();
        }



        //public void TerminateDownstreamTask()
        //{
        //    _cancelTokenSource.Cancel();
        //    _downstreamQueue.CompleteAdding();
        //}

        /// <summary>
        /// SetWatchdog should be called (if needed) before OpenChannel()
        /// </summary>
        public void SetWatchdog(IWatchdog wd)
        {
            _watchdog = wd;
        }

        public SL508DeviceManager(ISend<SendObject> targetConsumer, SL508892Setup setup, Session theSession)// : base(setup)
        {
            this._targetConsumer = targetConsumer;
            this._thisDeviceSetup = setup;
            this._serialSsession = theSession;
            this._deviceSlotIndex = setup.SlotNumber;
            this.DeviceName = DeviceMap2.SL508Literal;
        }

        public void Dispose()
        {
            if (true == _inDisposeFlag)
            {
                return;
            }
            _inDisposeFlag = true;

            _watchdog?.StopWatching();

            //TerminateDownstreamTask();

            _cancelTokenSource.Cancel();
            _downstreamQueue.CompleteAdding();

            _downstreamTask?.Wait();
            _downstreamTask = null;
#if usetasks
            //_cancelTokenSource.Cancel();
            var allReadTasks = _channelAuxList.Select(i => i.ReadTask);
            Task.WaitAll(allReadTasks.ToArray());
#else
            var readersWaitHandle = _channelAuxList.Select(i => i.AsyncResult.AsyncWaitHandle).ToArray();
            WaitHandle.WaitAll(readersWaitHandle);
#endif
            foreach (var cx in _channelAuxList)
            {
                cx.Reader.Dispose();
                cx.Writer.Dispose();
            }

            Console.WriteLine("Readers/writers disposed..");
        }
        public bool StartDevice()
        {
            if (_inDisposeFlag)
            {
                return false;
            }
            _channelAuxList = new List<ChannelAux>();
            ChannelStatList = new List<ChannelStat>();

            // build serial readers and writers
            // --------------------------------
            for (int chNum = 0; chNum < _serialSsession.GetNumberOfChannels(); chNum++)
            {
                // get channel index
                SerialPort sPort = _serialSsession.GetChannel(chNum) as SerialPort;
                int chIndex = sPort.GetIndex();

                // create reader & writer 
                var reader = new SerialReader(_serialSsession.GetDataStream(), chIndex);
                var writer = new SerialWriter(_serialSsession.GetDataStream(), chIndex);

                // add channel-aux to list
                ChannelAux chAux = new ChannelAux(chIndex, reader, writer, _serialSsession);
                _channelAuxList.Add(chAux);
                ChannelStatList.Add(new ChannelStat(chIndex));

                // register to WD service
                _watchdog?.Register($"Com{chIndex}", TimeSpan.FromSeconds(2.0)); // Hmm.. two second ... should use value relative to the value passed to .SetTimeout();
            }

            // start readers
            foreach (ChannelAux cx in _channelAuxList)
            {
#if usetasks
                cx.ReadTask = Task.Factory.StartNew(() => UpstreamMessageLoop_Task(cx), _cancelTokenSource.Token);
#else
                cx.AsyncResult = cx.Reader.BeginRead(200, new AsyncCallback(ReaderCallback), cx); // start reading from device
#endif
            }

            // start downstream message loop
            //_downstreamTask = Task.Factory.StartNew(DownstreamMessageLoop_Task, _cancelTokenSource.Token);

            //var allTasks = _channelAuxList.Where(cx => cx.ReadTask != null).Select(cx => cx.ReadTask);
            //return allTasks.ToArray()
            return true;
        }

        void ReaderCallback(IAsyncResult ar)
        {
            if (true == _inDisposeFlag)
            {
                return;
            }
            ChannelAux chAux = ar.AsyncState as ChannelAux;
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
                Console.WriteLine($"({++linenumber}) Message from channel {chIndex}. Length {recvBytes.Length}");
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
                        //Console.WriteLine($"Timeout ch {chIndex}");
                        _watchdog?.NotifyAlive(chName);
                        if (false == _inDisposeFlag)
                        {
                            chAux.AsyncResult = chAux.Reader.BeginRead(200, new AsyncCallback(ReaderCallback), chAux);
                        }
                    }
                    else
                    {
                        Console.WriteLine($"{chName} read error: {ex.Message}");
                        _watchdog?.NotifyCrash(chName);
                    }
                }
                System.Diagnostics.Debug.Assert(true == chAux.OriginatingSession.IsRunning());
            }
        }

        internal void WaitAll()
        {
            var allTasks = _channelAuxList.Where(cx => cx.ReadTask != null).Select(cx => cx.ReadTask);
            
            Task.WaitAll( allTasks.ToArray());
        }

        /// <summary>
        /// Ethernet to device message handler
        /// </summary>
        protected void HandleDownstreamRequest(EthernetMessage request)
        {
            if (true == _inDisposeFlag)
            {
                return;
            }

            int chIndex = request.SerialChannelNumber;
            ChannelAux cx = _channelAuxList.Where(i => i.ChannelIndex == chIndex).FirstOrDefault();
            if (null == cx)
            {
                Console.WriteLine("Message from Ethernet with non exists channel index. rejected");
                return;
            }
            UeiDaq.SerialWriter sw = cx.Writer;
            System.Diagnostics.Debug.Assert(sw != null);

            //
            try
            {
                int writtenBytes = 0;
                // write to serial port
                writtenBytes = sw.Write(request.PayloadBytes);
                System.Diagnostics.Debug.Assert(writtenBytes == request.PayloadBytes.Length);

                // wait state
                SerialPort sPort = _serialSsession.GetChannel(chIndex) as SerialPort;
                int sp = StaticMethods.GetSerialSpeedAsInt(sPort.GetSpeed());
                double bpsPossibe = Convert.ToDouble(sp) * 0.8;
                int bitsToSend = request.PayloadBytes.Length * 8;
                double waitstateMs = Convert.ToDouble(bitsToSend) / bpsPossibe * 1000.0; // in mili-sec
                System.Threading.Thread.Sleep(Convert.ToInt32(waitstateMs));

                // update counters
                ChannelStat chStat = ChannelStatList.Where(i => i.ChannelIndex == chIndex).FirstOrDefault();
                chStat.WrittenByteCount += writtenBytes;
                chStat.WrittenMessageCount++;

                //_ViewItemList[request.SerialChannelNumber] = new ViewItem<byte[]>(request.PayloadBytes, TimeSpan.FromSeconds(5));
            }
            catch (UeiDaqException ex)
            {
                _watchdog.NotifyCrash($"Com{cx.ChannelIndex}");
                Console.WriteLine($"UeiDaqException: {ex.Message}");
            }
            catch (Exception ex)
            {
                _watchdog.NotifyCrash($"Com{cx.ChannelIndex}");
                Console.WriteLine($"General exception: {ex.Message}");
            }
        }

        public string[] GetFormattedStatus(TimeSpan interval)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This task is per single channel (uart)
        /// </summary>
        /// <param name="cx"></param>
        protected void UpstreamMessageLoop_Task(ChannelAux cx)
        {

            // "available" is the amount of data remaining from a single event
            // more data may be in the queue once "available" bytes have been read

            int available = 0;
            string chName = $"Com{cx.ChannelIndex}";
            Console.WriteLine($"Started listening on {chName}");
            try
            {

                do //message loop
                {
                    do // wait for available messages
                    {
                        available = cx.OriginatingSession.GetDataStream().GetAvailableInputMessages(cx.ChannelIndex);
                        _watchdog?.NotifyAlive(chName);
                        if (available == 0)
                        {
                            System.Threading.Thread.Sleep(5);
                        }
                    } while ((available == 0)&&(false== _cancelTokenSource.IsCancellationRequested));

                    if (!_cancelTokenSource.IsCancellationRequested)
                    {
                        byte[] recvBytes = cx.Reader.Read(100); //ReadTimestamped()
                        Console.WriteLine($"({++linenumber}) Message from channel {cx.ChannelIndex}. Length {recvBytes.Length}");
                        ChannelStat chStat = ChannelStatList.Where(i => i.ChannelIndex == cx.ChannelIndex).FirstOrDefault();
                        chStat.ReadByteCount += recvBytes.Length;
                        chStat.ReadMessageCount++;

                    }
                } while (false == _cancelTokenSource.IsCancellationRequested);
            }
            catch (UeiDaqException e)
            {
                Console.WriteLine($"{chName} read exception. {e.Message}");
            }
            Console.WriteLine($"Stopped listening on {chName}");
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
                        string finalUri = $"{deviceSetup.CubeUrl}Dev{deviceSetup.SlotNumber}/Com{channelSetup.ChannelIndex}";
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

                // display channels info
                foreach (SerialPort ch in serialSession.GetChannels())
                {
                    Console.WriteLine($"Ch{ch.GetIndex()}   Mode:{ch.GetMode()}   Speed:{StaticMethods.GetSerialSpeedAsInt(ch.GetSpeed())}");
                }

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
