using System;
using System.Collections.Generic;
using System.Text;
using UeiDaq;
using UeiBridge.Types;
using UeiBridge.Library;
using System.Threading;
using System.Linq;

namespace UeiBridge
{
    /// <summary>
    /// "SL-508-892" manager.
    /// R&R: Reads from serial device and sends the result to 'targetConsumer'
    /// This class is responsible for disposing the serial session.
    /// </summary>
    class SL508InputDeviceManager : InputDevice
    {
        public override string DeviceName => DeviceMap2.SL508Literal; //"SL-508-892";

        private log4net.ILog _logger = StaticMethods.GetLogger();
        private readonly List<SerialReaderAdapter> _serialReaderList = new List<SerialReaderAdapter>();
        private bool _InDisposeState = false;
        private List<ViewItem<byte[]>> _lastScanList = new List<ViewItem<byte[]>>();
        private readonly SL508892Setup _thisSetup;
        //private ISend<SendObject> _targetConsumer;
        private SessionAdapter _serialSession;
        private List<IAsyncResult> _readerIAsyncResultList;
        private const int minLen = 200;
        

        public SL508InputDeviceManager(ISend<SendObject> targetConsumer, DeviceSetup setup, SessionAdapter serialSession) : base( setup)
        {
            _targetConsumer = targetConsumer;
            _thisSetup = setup as SL508892Setup;
            _serialSession = serialSession;

            System.Diagnostics.Debug.Assert(null != _targetConsumer);
            System.Diagnostics.Debug.Assert(null != _thisSetup);
            System.Diagnostics.Debug.Assert(null != serialSession);
            System.Diagnostics.Debug.Assert(this.DeviceName.Equals(setup.DeviceName));
        }

        public SL508InputDeviceManager() {}

        public override string [] GetFormattedStatus( TimeSpan interval)
        {
            List<string> resultList = new List<string>();
            for (int ch = 0; ch < _lastScanList.Count; ch++)
            {
                var item = _lastScanList[ch];
                if (null != item)
                {
                    if (item.TimeToLive > TimeSpan.Zero)
                    {
                        item.DecreaseTimeToLive( interval);
                        int len = (item.ReadValue.Length > 20) ? 20 : item.ReadValue.Length;
                        string s = $"Ch{ch}: Payload=({item.ReadValue.Length}): {BitConverter.ToString(item.ReadValue).Substring(0, len * 3 - 1)}";
                        resultList.Add(s);
                    }
                    else
                    {
                        item = null;
                    }
                    //else
                    //{
                    //    resultList.Add( $"Ch{ch}: <empty>");
                    //}
                }
                //else
                //{
                //    resultList.Add($"Ch{ch}: <empty>");
                //}
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

        public void ReaderCallback(IAsyncResult ar)
        {
            int channel = (int)ar.AsyncState;
            try
            {
                SerialReaderAdapter sra = _serialReaderList[channel];
                byte[] receiveBuffer = sra.EndRead(ar); 
                // this api might throw UeiDaqException exception with message "The device is not responding, check the connection and the device's status"
                // in this case, the session must be closed/disposed and open again.

                _lastScanList[channel] = new ViewItem<byte[]>(receiveBuffer, TimeSpan.FromSeconds(5));
                EthernetMessage em = StaticMethods.BuildEthernetMessageFromDevice(receiveBuffer, this._thisSetup, channel);
                _targetConsumer.Send(new SendObject( _thisSetup.DestEndPoint.ToIpEp(), em.GetByteArray( MessageWay.upstream)));

                // restart reader
                if (_InDisposeState == false)
                {
                    System.Diagnostics.Debug.Assert(true == _serialSession.IsRunning());
                    _readerIAsyncResultList[channel] = _serialReaderList[channel].BeginRead(minLen, this.ReaderCallback, channel);
                }
            }
            catch (UeiDaqException ex)
            {
                if (Error.Timeout == ex.Error)
                {
                    if (_InDisposeState == false)
                    {
                        System.Diagnostics.Debug.Assert(true == _serialSession.IsRunning());
                        _readerIAsyncResultList[channel] = _serialReaderList[channel].BeginRead(minLen, this.ReaderCallback, channel);
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
                }
            }
            catch(Exception ex)
            {
                _logger.Warn($"ReaderCallback: {InstanceName}. {ex.Message}");
            }
        }
        public override bool OpenDevice()
        {
            _lastScanList = new List<ViewItem<byte[]>>(new ViewItem<byte[]>[_serialSession.GetNumberOfChannels()]);
            _readerIAsyncResultList = new List<IAsyncResult>(new IAsyncResult[_thisSetup.Channels.Count]);

            // build reader list 
            for (int ch = 0; ch < _serialSession.GetNumberOfChannels(); ch++)
            {
                var sr = _serialSession.GetSerialReader(ch);
                _serialReaderList.Add( sr);
            }
            System.Threading.Thread.Sleep(10);

            // activate
            int ch1 = 0;
            foreach (var sl in _serialReaderList)
            {
                AsyncCallback readerAsyncCallback = new AsyncCallback(ReaderCallback);
                _readerIAsyncResultList[ch1] = sl.BeginRead(minLen, readerAsyncCallback, ch1);
                ch1++;
            }

            EmitInitMessage( $"Init success {DeviceName}. {_serialSession.GetNumberOfChannels()} channels. Dest:{ _thisSetup.DestEndPoint.ToIpEp()}");

            return true;
        }
        public override void Dispose()
        {
            _InDisposeState = true;
            _serialSession.Stop();
            var waitall = _readerIAsyncResultList.Select(i => i.AsyncWaitHandle).ToArray();
            WaitHandle.WaitAll(waitall);
            for (int ch = 0; ch < _serialReaderList.Count; ch++)
            {
                _serialReaderList[ch].Dispose();
            }
            _serialSession.Dispose();
            _targetConsumer.Dispose();

            _logger.Debug($"{this.DeviceName}/Input, slot {_thisSetup.SlotNumber}, Disposed");
        }
    }
}
