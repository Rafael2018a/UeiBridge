using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UeiBridge;
using UeiBridge.CubeSetupTypes;
using UeiBridge.Interfaces;
using UeiBridge.Types;
using UeiDaq;

namespace SerialOp
{
    class SL508InputManager : InputDevice
    {
        public override string DeviceName => throw new NotImplementedException();
        SL508892Setup serialDev;
        Session _ueiSsession;
        List<ChannelAux> _channelAuxList;
        bool _inDisposeFlag = false;
        IWatchdog _watchdog;
        /// <summary>
        /// SetWatchdog should be called before OpenChannel()
        /// </summary>
        /// <param name="wd"></param>
        public void SetWatchdog(IWatchdog wd)
        {
            _watchdog = wd;
        }
        public SL508InputManager(ISend<SendObject> targetConsumer, SL508892Setup setup, Session theSession) : base(setup)
        {
            this._targetConsumer = targetConsumer;
            this.serialDev = setup;
            this._ueiSsession = theSession;
        }
        public override void Dispose()
        {
            if (true==_inDisposeFlag)
            {
                return;
            }
            _inDisposeFlag = true;
            _watchdog?.StopWatching();
            Console.WriteLine("Waiting on all readers..");
            var waitall = _channelAuxList.Select(i => i.AsyncResult.AsyncWaitHandle).ToArray();
            WaitHandle.WaitAll(waitall);
            foreach (var cx in _channelAuxList)
            {
                cx.Reader.Dispose();
            }
            Console.WriteLine("Readers disposed..");
        }
        public override string[] GetFormattedStatus(TimeSpan interval)
        {
            throw new NotImplementedException();
        }
        public override bool OpenDevice()
        {
            if (_inDisposeFlag)
            {
                return false;
            }
            _channelAuxList = new List<ChannelAux>();
            for (int chNum = 0; chNum < _ueiSsession.GetNumberOfChannels(); chNum++)
            {
                // set channel properties
                SerialPort sPort = _ueiSsession.GetChannel(chNum) as SerialPort;
                int chIndex = sPort.GetIndex();
                SerialChannelSetup serialChannel = serialDev.Channels[chIndex]; // tbd. use GetChannelEntry()
                System.Diagnostics.Debug.Assert(null != serialChannel);
                sPort.SetMode(serialChannel.Mode);
                sPort.SetSpeed(serialChannel.Baudrate);
                sPort.SetParity(serialChannel.Parity);
                sPort.SetStopBits(serialChannel.Stopbits);

                // add channel to channel-list
                ChannelAux chAux = new ChannelAux(chIndex, _ueiSsession);
                _channelAuxList.Add(chAux);
                // set serial reader 
                chAux.Reader = new SerialReader(_ueiSsession.GetDataStream(), chAux.ChannelIndex);
                // register to WD service
                _watchdog?.Register($"Com{chIndex}", TimeSpan.FromSeconds(1.0)); // Hmm.. one second ... should use twice the value passed to .SetTimeout();
            }
            // start readers
            foreach (ChannelAux cx in _channelAuxList)
            {
                cx.AsyncResult = cx.Reader.BeginRead(200, new AsyncCallback(ReaderCallback), cx);
            }

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

    }
}
