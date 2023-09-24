﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
        List<CANReaderAdapter> _canReaderList = new List<CANReaderAdapter>();
        private List<IAsyncResult> _readerIAsyncResultList = new List<IAsyncResult>();
        CAN503Setup _thisSetup;
        bool _inDisposeState = false;
        
        public CAN503InputDeviceManager()
        {
        }

        public CAN503InputDeviceManager(CAN503Setup setup, SessionAdapter ssAdapter, UdpWriter uWriter) : base(setup)
        {
            this._thisSetup = setup;
            this._ueiSession = ssAdapter;
            //this._udpWriter = uWriter;
            this._targetConsumer = uWriter;
        }

        public override string DeviceName => DeviceMap2.CAN503Literal;

        public override void Dispose()
        {
            _inDisposeState = true;
            _ueiSession.Stop();
            var waitall = _readerIAsyncResultList.Select(i => i.AsyncWaitHandle).ToArray();
            WaitHandle.WaitAll(waitall);
            for (int ch=0; ch<_canReaderList.Count; ch++)
            {
                _canReaderList[ch].Dispose();
            }
            _ueiSession.Dispose();
            _targetConsumer.Dispose();
            
            _logger.Debug($"{this.DeviceName}/Input, slot {_thisSetup.SlotNumber}, Disposed");
        }

        public override string[] GetFormattedStatus(TimeSpan interval)
        {
            return null;
        }

        public override bool OpenDevice()
        {
            for (int ch = 0; ch < _ueiSession.GetNumberOfChannels(); ch++)
            {
                CANReaderAdapter cr = _ueiSession.GetCANReader(ch);
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
                CANReaderAdapter cra = _canReaderList[channel];
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
                    System.Diagnostics.Debug.Assert(true == _ueiSession.IsRunning());
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
                        System.Diagnostics.Debug.Assert(true == _ueiSession.IsRunning());
                        _readerIAsyncResultList[channel] = _canReaderList[channel].BeginRead( 1, this.ReaderCallback, channel);
                    }
                    else
                    {
                        _logger.Debug($"Disposing {InstanceName} ch{channel}");
                        // tbd. Dispose session here?
                    }
                }
                else
                {
                    _logger.Warn($"ReaderCallback:  {InstanceName}. {ex.Message}.");

                    // tbd. Dispose session here?
                }
            }
            catch (Exception ex)
            {
                _logger.Warn($"ReaderCallback: {InstanceName}. {ex.Message}");
            }
        }

    }
}