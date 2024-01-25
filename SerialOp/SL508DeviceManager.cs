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

namespace SerialOp
{
    /// <summary>
    /// Manage serial device.
    /// - Handle upstream/downstream messages 
    /// - update a given watchdog
    /// - maintain channel statistics
    /// </summary>
    public class SL508DeviceManager : DeviceManagerBase
    {
        private ISend<SendObject> _targetConsumer;
        SL508892Setup _thisDeviceSetup;
        Session _serialSsession;
        List<ChannelAux> _channelAuxList; // note that the index of this list is NOT (necessarily) the channel index
        public List<ChannelStat> ChannelStatList { get; private set; } // note that the index of this list is NOT (necessarily) the channel index
        IWatchdog _watchdog;

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

        public override void Dispose()
        {
            if (true==_inDisposeFlag)
            {
                return;
            }
            _inDisposeFlag = true;
            
            _watchdog?.StopWatching();


            
            TerminateDownstreamTask();
            _downstreamTask.Wait();
            _downstreamTask = null;

            var readersWaitHandle = _channelAuxList.Select(i => i.AsyncResult.AsyncWaitHandle).ToArray();
            WaitHandle.WaitAll(readersWaitHandle);
            foreach (var cx in _channelAuxList)
            {
                cx.Reader.Dispose();
                cx.Writer.Dispose();
            }

            Console.WriteLine("Readers/writers disposed..");
        }
        
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
                Console.WriteLine($"Com {chIndex}: {sPort1.GetMode()} {sPort1.GetSpeed()}" );
                
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
                Console.WriteLine($"Message from channel {chIndex}. Length {recvBytes.Length}");
                _watchdog?.NotifyAlive( chName);
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

        /// <summary>
        /// Ethernet to device message handler
        /// </summary>
        protected override void HandleDownstreamRequest(EthernetMessage request)
        {
            if (true == _inDisposeFlag)
            {
                return;
            }

            int chIndex = request.SerialChannelNumber;
            ChannelAux cx = _channelAuxList.Where(i => i.ChannelIndex == chIndex).FirstOrDefault();
            if (null==cx)
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
                System.Diagnostics.Debug.Assert( writtenBytes == request.PayloadBytes.Length);

                // wait state
                SerialPort sPort = _serialSsession.GetChannel( chIndex) as SerialPort;
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

        public override string[] GetFormattedStatus(TimeSpan interval)
        {
            throw new NotImplementedException();
        }
    }
}
