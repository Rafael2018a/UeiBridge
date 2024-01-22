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
    /// This class contains methods and fields which are common to all device managers
    /// </summary>
    public abstract class DeviceManagerBase : IDeviceManager
    {
        private BlockingCollection<EthernetMessage> _ethToDeviceQueue2 = new BlockingCollection<EthernetMessage>(100); // max 100 items
        abstract protected void HandleEthToDeviceRequest(EthernetMessage request);
        public string DeviceName { get; protected set;}
        public string InstanceName { get; protected set;}
        protected int _deviceSlotIndex;
        protected bool _isOutputDeviceReady = true;
        protected bool stopTask = false;  // tbd. use cancel-token
        protected bool _inDisposeFlag = false;

        /// <summary>
        /// Push etherent message to EthToDevice message queue
        /// </summary>
        public void Enqueue(byte[] m)
        {
            if (_ethToDeviceQueue2.IsCompleted)
            {
                return;
            }

            try
            {
                string err = null;
                EthernetMessage em = EthernetMessage.CreateFromByteArray(m, MessageWay.downstream, ref err);
                if (null == em)
                {
                    Console.WriteLine(err);
                    return;
                }

                if (false == _ethToDeviceQueue2.TryAdd(em))
                {
                    Console.WriteLine($"Incoming message dropped due to full messae queue");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Incoming message dropped. {ex.Message}.");
            }

        }

        protected void EthToDeviceMessageLoop_Task()// Action<EthernetMessage> handleRequsetAction)
        {
            // message loop
            // ============
            while ((false == _ethToDeviceQueue2.IsCompleted)&&(stopTask==false))
            {
                try
                {
                    EthernetMessage incomingMessage = _ethToDeviceQueue2.Take(); // get from q

                    if (null == incomingMessage) // end task token
                    {
                        _ethToDeviceQueue2.CompleteAdding();
                        break;
                    }

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
                    if (_ethToDeviceQueue2.Count == _ethToDeviceQueue2.BoundedCapacity)
                    {
                        Console.WriteLine($"Input queue items = {_ethToDeviceQueue2.Count}");
                    }

                    // finally, Handle message
                    if (_isOutputDeviceReady)
                    {
                        HandleEthToDeviceRequest(incomingMessage);
                    }
                    else
                    {
                        Console.WriteLine($"Device {DeviceName} not ready. message rejected.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        public abstract string[] GetFormattedStatus(TimeSpan interval);
    }

    public class SL508DeviceManager : DeviceManagerBase
    {
        //public override string DeviceName => throw new NotImplementedException();
        SL508892Setup thisDeviceSetup;
        Session _serialSsession;
        List<ChannelAux> _channelAuxList;
        IWatchdog _watchdog;
        int _numberOfSentBytes;
        int _numberOfSentMessages;
        Task messageLoopTask;

        /// <summary>
        /// SetWatchdog should be called before OpenChannel()
        /// </summary>
        public void SetWatchdog(IWatchdog wd)
        {
            _watchdog = wd;
        }

        public SL508DeviceManager(ISend<SendObject> targetConsumer, SL508892Setup setup, Session theSession)// : base(setup)
        {
            //this._targetConsumer = targetConsumer;
            this.thisDeviceSetup = setup;
            this._serialSsession = theSession;
            this._deviceSlotIndex = setup.SlotNumber;

            this.DeviceName = DeviceMap2.SL508Literal;
        }

        public void Dispose()
        {
            if (true==_inDisposeFlag)
            {
                return;
            }
            _inDisposeFlag = true;
            stopTask = true;
            Console.WriteLine("_inDisposeFlag = true");
            _watchdog?.StopWatching();
            
            Console.WriteLine("Waiting on channel readers to dispose");
            messageLoopTask = null;
            var readersWaitHandle = _channelAuxList.Select(i => i.AsyncResult.AsyncWaitHandle).ToArray();
            WaitHandle.WaitAll(readersWaitHandle);
            foreach (var cx in _channelAuxList)
            {
                cx.Reader.Dispose();
                cx.Writer.Dispose();
            }
            Console.WriteLine("Readers/writers disposed..");
        }
        
        //public override string[] GetFormattedStatus(TimeSpan interval)
        //{
        //    throw new NotImplementedException();
        //}
        public bool OpenDevice()
        {
            if (_inDisposeFlag)
            {
                return false;
            }
            _channelAuxList = new List<ChannelAux>();

            // set serial channels and add them to channel list
            // ------------------------------------------------
            for (int chNum = 0; chNum < _serialSsession.GetNumberOfChannels(); chNum++)
            {
                // set channel properties
                SerialPort sPort = _serialSsession.GetChannel(chNum) as SerialPort;
                int chIndex = sPort.GetIndex();
                SerialChannelSetup serialChannel = thisDeviceSetup.GetChannelEntry(chIndex);  
                System.Diagnostics.Debug.Assert(null != serialChannel);
                sPort.SetMode(serialChannel.Mode);
                sPort.SetSpeed(serialChannel.Baudrate);
                sPort.SetParity(serialChannel.Parity);
                sPort.SetStopBits(serialChannel.Stopbits);

                // set reader & writer and add channel to channel-list
                var reader = new SerialReader(_serialSsession.GetDataStream(), chIndex);
                var writer = new SerialWriter(_serialSsession.GetDataStream(), chIndex);
                ChannelAux chAux = new ChannelAux(chIndex, reader, writer, _serialSsession);
                _channelAuxList.Add(chAux);

                // register to WD service
                _watchdog?.Register($"Com{chIndex}", TimeSpan.FromSeconds(2.0)); // Hmm.. two second ... should use value relative to the value passed to .SetTimeout();
            }

            // start readers
            foreach (ChannelAux cx in _channelAuxList)
            {
                cx.AsyncResult = cx.Reader.BeginRead(200, new AsyncCallback(ReaderCallback), cx); // start reading from device
            }

            stopTask = false;
            messageLoopTask = Task.Factory.StartNew(EthToDeviceMessageLoop_Task);

            return false;
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

                Console.WriteLine($"Message from channel {chIndex}. Length {recvBytes.Length}");
                _watchdog?.NotifyAlive( chName);
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

        protected override void HandleEthToDeviceRequest(EthernetMessage request)
        {
            if (true == _inDisposeFlag)
            {
                return;
            }

            System.Diagnostics.Debug.Assert(request.SerialChannelNumber < _channelAuxList.Count);
            ChannelAux cx = _channelAuxList[request.SerialChannelNumber];
            UeiDaq.SerialWriter sw = cx.Writer;
            System.Diagnostics.Debug.Assert(sw != null);

            int sentBytes = 0;
            try
            {
                // write to serial port
                sentBytes = sw.Write(request.PayloadBytes);
                System.Diagnostics.Debug.Assert(sentBytes == request.PayloadBytes.Length);

                // wait state
                SerialPort sPort = _serialSsession.GetChannel(request.SerialChannelNumber) as SerialPort;
                int sp = StaticMethods.GetSerialSpeedAsInt(sPort.GetSpeed());
                double bpsPossibe = Convert.ToDouble(sp) * 0.8;
                int bitsToSend = request.PayloadBytes.Length * 8;
                double waitstateMs = Convert.ToDouble(bitsToSend) / bpsPossibe * 1000.0; // in milisec
                System.Threading.Thread.Sleep(Convert.ToInt32(waitstateMs));
                
                // update counters
                
                _numberOfSentBytes += sentBytes;
                _numberOfSentMessages++;
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

        public override string[] GetFormattedStatus(TimeSpan interval)
        {
            throw new NotImplementedException();
        }
    }
}
