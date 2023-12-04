using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UeiBridge.CubeSetupTypes;
using UeiBridge.Interfaces;
using UeiBridge.Library;
using UeiBridge.Types;
using UeiDaq;

namespace UeiBridge
{
    class CAN503InputDeviceManager : InputDevice
    {
        //private SessionAdapter _sessionAdapter;
        //private UdpWriter _udpWriter;
        private log4net.ILog _logger = StaticMethods.GetLogger();
        List<ICANReaderAdapter> _canReaderList = new List<ICANReaderAdapter>();
        private List<IAsyncResult> _readerIAsyncResultList = new List<IAsyncResult>();
        CAN503Setup _thisSetup;
        bool _inDisposeState = false;
        
        public CAN503InputDeviceManager()
        {
        }

        public CAN503InputDeviceManager(CAN503Setup setup, SessionAdapter ssAdapter, UdpWriter uWriter) : base(setup)
        {
            this._thisSetup = setup;
            this._iSession = ssAdapter;
            //this._udpWriter = uWriter;
            this._targetConsumer = uWriter;
        }

        public override string DeviceName => DeviceMap2.CAN503Literal;

        public override void Dispose()
        {
            _inDisposeState = true;
            _iSession.Stop();
            var waitall = _readerIAsyncResultList.Select(i => i.AsyncWaitHandle).ToArray();
            WaitHandle.WaitAll(waitall);
            for (int ch=0; ch<_canReaderList.Count; ch++)
            {
                _canReaderList[ch].Dispose();
            }
            _iSession.Dispose();
            _targetConsumer.Dispose();
            
            _logger.Debug($"{this.DeviceName}/Input, slot {_thisSetup.SlotNumber}, Disposed");
        }

        public override string[] GetFormattedStatus(TimeSpan interval)
        {
            return null;
        }

        public override bool OpenDevice()
        {
            for (int ch = 0; ch < _iSession.GetNumberOfChannels(); ch++)
            {
                ICANReaderAdapter cr = _iSession.GetCANReader(ch);
                AsyncCallback readCallback = new AsyncCallback(ReaderCallback);
                _readerIAsyncResultList.Add(cr.BeginRead(1, readCallback, ch));
                _canReaderList.Add(cr);
            }

            return false;
        }

        public void ReaderCallback(IAsyncResult ar)
        {
            int channel = (int)ar.AsyncState;
            try
            {
                ICANReaderAdapter cra = _canReaderList[channel];
                UeiDaq.CANFrame[] framesBuffer = cra.EndRead(ar);
                System.Diagnostics.Debug.Assert( framesBuffer.Length == 1);

                //_lastScanList[channel] = new ViewItem<byte[]>(receiveBuffer, TimeSpan.FromSeconds(5));
                //byte[] payload = receiveBuffer;
                EthernetMessage em = StaticMethods.BuildEthernetMessageFromDevice( framesBuffer, this._thisSetup, channel);
                // forward to consumer (send by udp)
                _targetConsumer.Send(new SendObject( _thisSetup.DestEndPoint.ToIpEp(), em.GetByteArray(MessageWay.upstream)));
                
                // restart reader
                if (_inDisposeState == false)
                {
                    System.Diagnostics.Debug.Assert(true == _iSession.IsRunning());
                    _readerIAsyncResultList[channel] = _canReaderList[channel].BeginRead( 1, this.ReaderCallback, channel);
                }
            }
            catch (UeiDaqException ex)
            {
                if (Error.Timeout == ex.Error)
                {
                    // Ignore timeout error, they will occur if the send button is not
                    // clicked on fast enough!
                    if (_inDisposeState == false)
                    {
                        System.Diagnostics.Debug.Assert(true == _iSession.IsRunning());
                        _readerIAsyncResultList[channel] = _canReaderList[channel].BeginRead( 1, this.ReaderCallback, channel);
                    }
                    else
                    {
                        _logger.Debug($"Disposing {InstanceName} ch{channel}");
                    }
                }
                else
                {
                    _logger.Warn($"ReaderCallback:  {InstanceName}. {ex.Message}.");
                }
            }
            catch (Exception ex)
            {
                _logger.Warn($"ReaderCallback: {InstanceName}. {ex.Message}");
            }
        }

    }
}
