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
    /// </summary>
    class SL508InputDeviceManager : InputDevice
    {
        public override string DeviceName => "SL-508-892";
        public bool InDisposeState => _InDisposeState;

        private log4net.ILog _logger = StaticMethods.GetLogger();
        private readonly List<SerialPort> _serialPorts;
        private readonly List<SerialReader> _serialReaderList;
        private IConvert _attachedConverter;
        private bool _InDisposeState = false;
        private List<ViewItem<byte[]>> _lastScanList;
        private readonly System.Net.IPEndPoint _targetEp;
        private readonly SL508892Setup _thisDeviceSetup;
        private ISend<SendObject> _targetConsumer;
        private SessionEx _serialSession;
        private List<IAsyncResult> _readerIAsyncResultList;

        public SL508InputDeviceManager(ISend<SendObject> targetConsumer, DeviceSetup setup, SessionEx serialSession) : base( setup)
        {
            _serialPorts = new List<SerialPort>();
            _serialReaderList = new List<SerialReader>();
            //_serialWriterList = new List<SerialWriter>();
            _attachedConverter = StaticMethods.CreateConverterInstance(setup);
            _lastScanList = new List<ViewItem<byte[]>>();
            _targetConsumer = targetConsumer;
           
            _serialSession = serialSession;
            _targetEp = setup.DestEndPoint.ToIpEp();

            _thisDeviceSetup = setup as SL508892Setup;

            System.Diagnostics.Debug.Assert(null != serialSession);
            System.Diagnostics.Debug.Assert(null != setup);
            System.Diagnostics.Debug.Assert(this.DeviceName.Equals(setup.DeviceName));
        }

        public SL508InputDeviceManager() 
        {
        }

        public override string [] GetFormattedStatus( TimeSpan interval)
        {
            //StringBuilder formattedString = new StringBuilder();
            List<string> resultList = new List<string>();
            for (int ch = 0; ch < _lastScanList.Count; ch++)
            {
                var item = _lastScanList[ch];
                if (null != item)
                {
                    if (item.timeToLive.Ticks > 0)
                    {
                        item.timeToLive -= interval;
                        int len = (item.readValue.Length > 20) ? 20 : item.readValue.Length;
                        string s = $"Ch{ch}: Payload=({item.readValue.Length}): {BitConverter.ToString(item.readValue).Substring(0, len * 3 - 1)}";
                        resultList.Add(s);
                        //formattedString.Append(s);
                    }
                    else
                    {
                        resultList.Add( $"Ch{ch}: <empty>");
                    }
                }
                else
                {
                    resultList.Add($"Ch{ch}: <empty>");
                }
            }
            return resultList.ToArray();
        }
        //AsyncCallback readerAsyncCallback;
        private IAsyncResult readerIAsyncResult;
        int minLen = 200;

        public void ReaderCallback(IAsyncResult ar)
        {
            System.Diagnostics.Debug.Assert(null != _serialSession);
            int channel = (int)ar.AsyncState;
            //_logger.Debug($"(int)ar.AsyncState; {channel}");
            try
            {
                byte[] receiveBuffer = _serialReaderList[channel].EndRead(ar);

                _lastScanList[channel] = new ViewItem<byte[]>(receiveBuffer, timeToLiveMs: 5000);
                //_logger.Debug($"read from serial port. ch {channel}");
                byte [] payload = this._attachedConverter.DeviceToEth(receiveBuffer);
                EthernetMessage em = StaticMethods.BuildEthernetMessageFromDevice(payload, this._thisDeviceSetup, channel);
                // forward to consumer (send by udp)
                //ScanResult sr = new ScanResult(receiveBuffer, this);
                _targetConsumer.Send(new SendObject(_targetEp, em.GetByteArray( MessageWay.upstream)));

                // restart reader
                _readerIAsyncResultList[channel] = _serialReaderList[channel].BeginRead(minLen, this.ReaderCallback, channel);
            }
            catch (UeiDaqException ex)
            {
                if (Error.Timeout == ex.Error)
                {
                    // Ignore timeout error, they will occur if the send button is not
                    // clicked on fast enough!
                    if (_InDisposeState == false)
                    {
                        _readerIAsyncResultList[channel] = _serialReaderList[channel].BeginRead(minLen, this.ReaderCallback, channel);
                    }
                    else
                    {
                        _logger.Debug($"Not resuming channel-read since disposing started. {InstanceName} ch{channel}");
                    }
                }
                else
                {
                    _logger.Warn($"ReaderCallback: {ex.Message}");
                }
            }
            catch(Exception ex)
            {
                _logger.Warn($"ReaderCallback: {ex.Message}");
            }
        }
        public override void OpenDevice()
        {
            if (_serialSession == null)
            {
                _logger.Warn($"Failed to open device {this.InstanceName}");
                return;
            }

            _lastScanList = new List<ViewItem<byte[]>>(new ViewItem<byte[]>[_serialSession.GetNumberOfChannels()]);
            _readerIAsyncResultList = new List<IAsyncResult>(new IAsyncResult[_thisDeviceSetup.Channels.Count]);

            
            //int ch = 0;
            for (int ch = 0; ch < _serialSession.GetNumberOfChannels(); ch++)
            {
                
                var sr = new SerialReader(_serialSession.GetDataStream(), _serialSession.GetChannel(ch).GetIndex());
                //readerAsyncCallback = new AsyncCallback(ReaderCallback);
                //sr.BeginRead(minLen, readerAsyncCallback, ch);
                _serialReaderList.Add(sr);
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
            //for (int i = 0; i < _serialSession.GetNumberOfChannels(); i++)
            //{
            //    _lastScanList.Add(null);
            //}

            _logger.Info($"Init success {InstanceName}. {_serialSession.GetNumberOfChannels()} channels. Dest:{_targetEp}");

        }
        public override void Dispose()
        {
            _InDisposeState = true;

            var waitall = _readerIAsyncResultList.Select(i => i.AsyncWaitHandle).ToArray();
            WaitHandle.WaitAll(waitall);

            _logger.Debug($"Disposing {this.DeviceName}/Input, slot {_thisDeviceSetup.SlotNumber}");
            if (_serialSession.IsRunning())
            {
                _serialSession.Stop();
            }
            _serialSession.Dispose();

        }
    }
}
