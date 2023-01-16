using System;
using System.Collections.Generic;
using System.Text;
using UeiDaq;
using UeiBridgeTypes;

namespace UeiBridge
{

    /// <summary>
    /// "SL-508-892" manager.
    /// R&R: Reads from serial device and sends the result to 'targetConsumer'
    /// </summary>
    class SL508InputDeviceManager : InputDevice
    {
        log4net.ILog _logger = StaticMethods.GetLogger();
        public override string DeviceName => "SL-508-892";
        readonly List<SerialPort> _serialPorts;
        readonly List<SerialReader> _serialReaderList;
        IConvert _attachedConverter;
        bool _InDisposeState = false;
        List<ViewerItem<byte[]>> _lastScanList;
        readonly System.Net.IPEndPoint _targetEp;

        public override IConvert AttachedConverter => _attachedConverter;
        //public List<SerialWriter> SerialWriterList => _serialWriterList;
        public bool InDisposeState => _InDisposeState;

        public override string InstanceName { get; }

        private Session _serialSession;
        public SL508InputDeviceManager(ISend<SendObject> targetConsumer, DeviceSetup setup, Session serialSession) : base(targetConsumer)
        {
            _serialPorts = new List<SerialPort>();
            _serialReaderList = new List<SerialReader>();
            //_serialWriterList = new List<SerialWriter>();
            _attachedConverter = StaticMethods.CreateConverterInstance(setup);
            _lastScanList = new List<ViewerItem<byte[]>>();
            System.Diagnostics.Debug.Assert(null != serialSession);
            _serialSession = serialSession;
            InstanceName = $"{DeviceName}/Slot{setup.SlotNumber}/Input";
            _targetEp = setup.DestEndPoint.ToIpEp();
        }

        public SL508InputDeviceManager() : base(null)
        {
        }

        public override string GetFormattedStatus( TimeSpan interval)
        {
            StringBuilder formattedString = new StringBuilder();
            for (int ch = 0; ch < _lastScanList.Count; ch++)
            {
                var item = _lastScanList[ch];
                if (null != item)
                {
                    if (item.timeToLive.Ticks > 0)
                    {
                        item.timeToLive -= interval;
                        int len = (item.readValue.Length > 20) ? 20 : item.readValue.Length;
                        string s = $"Ch{ch}: Payload=({item.readValue.Length}): {BitConverter.ToString(item.readValue).Substring(0, len * 3 - 1)}\n";
                        formattedString.Append(s);
                    }
                    else
                    {
                        formattedString.Append($"Ch{ch}: <empty>\n");
                    }
                }
                else
                {
                    formattedString.Append($"Ch{ch}: <empty>\n");
                }
            }
            return formattedString.ToString();
        }
        AsyncCallback readerAsyncCallback;
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

                _lastScanList[channel] = new ViewerItem<byte[]>(receiveBuffer, timeToLiveMs: 5000);
                //_logger.Debug($"read from serial port. ch {channel}");
                byte [] payload = this.AttachedConverter.DeviceToEth(receiveBuffer);
                EthernetMessage em = EthernetMessage.CreateFromDevice(payload, this.DeviceName);
                // forward to consumer (send by udp)
                //ScanResult sr = new ScanResult(receiveBuffer, this);
                _targetConsumer.Send(new SendObject(_targetEp, em.ToByteArrayUp()));

                // restart reader
                readerIAsyncResult = _serialReaderList[channel].BeginRead(minLen, this.ReaderCallback, channel);
            }
            catch (UeiDaqException ex)
            {
                if (Error.Timeout == ex.Error)
                {
                    // Ignore timeout error, they will occur if the send button is not
                    // clicked on fast enough!
                    readerIAsyncResult = _serialReaderList[channel].BeginRead(minLen, this.ReaderCallback, channel);
                }
                else
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
        public override void OpenDevice()
        {
            System.Threading.Thread.Sleep(100);
            //int ch = 0;
            for (int ch = 0; ch < _serialSession.GetNumberOfChannels(); ch++)
            {
                System.Threading.Thread.Sleep(10);
                var sr = new SerialReader(_serialSession.GetDataStream(), _serialSession.GetChannel(ch).GetIndex());
                readerAsyncCallback = new AsyncCallback(ReaderCallback);
                sr.BeginRead(minLen, readerAsyncCallback, ch);
                _serialReaderList.Add(sr);
            }
            for (int i = 0; i < _serialSession.GetNumberOfChannels(); i++)
            {
                _lastScanList.Add(null);
            }

            _logger.Info($"Init success {InstanceName}. {_serialSession.GetNumberOfChannels()} channels. Dset:{_targetEp}");

        }
        public override void Dispose()
        {
            _InDisposeState = true;
            for (int i = 0; i < _serialReaderList.Count; i++)
            {
                
                _serialReaderList[i].Dispose();
            }
            //if (_serialSession.IsRunning())
            //{
            //    _deviceSession?.Stop();
            //}
            _deviceSession?.Dispose();

        }

        [Obsolete]
        private bool OpenDevices(string baseUrl, string deviceName)
        {
            _deviceSession = new Session();

            foreach (var channel in Config.Instance.SerialChannels)
            {
                string finalUrl = baseUrl + channel.portname.ToString();
                //string finalUrl = baseUrl + "Com0,1";
                var port = _deviceSession.CreateSerialPort(finalUrl,
                                    SerialPortMode.RS232,
                                    SerialPortSpeed.BitsPerSecond250000,
                                    SerialPortDataBits.DataBits8,
                                    SerialPortParity.None,
                                    SerialPortStopBits.StopBits1,
                                    "");
                _serialPorts.Add(port);
            }

            int numberOfChannels = _deviceSession.GetNumberOfChannels();
            System.Diagnostics.Debug.Assert(numberOfChannels == Config.Instance.SerialChannels.Length);

            _deviceSession.ConfigureTimingForMessagingIO(1000, 100.0);
            _deviceSession.GetTiming().SetTimeout(5000); // timeout to throw from _serialReader.EndRead (looks like default is 1000)
            //_deviceSession.ConfigureTimingForSimpleIO();

            {
                _deviceSession.GetChannels();
            }

            for (int ch = 0; ch < numberOfChannels; ch++)
            {
                System.Threading.Thread.Sleep(10);
                SerialReader sr = new SerialReader(_deviceSession.GetDataStream(), _deviceSession.GetChannel(ch).GetIndex());
                _serialReaderList.Add(sr);
                SerialWriter sw = new SerialWriter(_deviceSession.GetDataStream(), _deviceSession.GetChannel(ch).GetIndex());
                //_serialWriterList.Add(sw);

            }
            _deviceSession.Start();

            for (int ch = 0; ch < numberOfChannels; ch++)
            {
                System.Threading.Thread.Sleep(10);
                readerAsyncCallback = new AsyncCallback(ReaderCallback);
                readerIAsyncResult = _serialReaderList[ch].BeginRead(minLen, readerAsyncCallback, ch);
            }

            bool firstIteration = true;
            foreach (SerialPort port in _serialPorts)
            {
                if (firstIteration)
                {
                    firstIteration = false;
                    _logger.Info($"*** {DeviceName} init:");
                }
                int channleIndex = port.GetIndex();
                _logger.Info($"Serial CH{channleIndex} init success. {port.GetMode()}  {port.GetSpeed()}.");////{c111.GetResourceName()}");
            }

            System.Threading.Thread.Sleep(500);
            return true;
        }

    }
}
