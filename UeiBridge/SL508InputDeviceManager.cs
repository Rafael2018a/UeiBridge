using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UeiDaq;

namespace UeiBridge
{

    /// <summary>
    /// "SL-508-892"
    /// </summary>
    class SL508InputDeviceManager : InputDevice
    {
        log4net.ILog _logger = StaticMethods.GetLogger();
        //SL508Input _serialInput;
        //SL508OutputDeviceManager _serialOutput;
        public override string DeviceName => "SL-508-892";
        //readonly new string _channelsString;
        readonly List<SerialPort> _serialPorts;
        readonly List<SerialReader> _serialReaderList;
        readonly List<SerialWriter> _serialWriterList;
        IConvert _attachedConverter;
        //private SerialReader _serialReader;
        //private SerialWriter _serialWriter;
        bool _InDisposeState = false;
        byte[] _lastMessage;

        //readonly List<Session> _deviceSessionList;
        string _channelBaseString;

        public override IConvert AttachedConverter => _attachedConverter;
        public List<SerialWriter> SerialWriterList => _serialWriterList;
        public bool InDisposeState => _InDisposeState;
        //IEnqueue<ScanResult> _targetConsumer;

        public SL508InputDeviceManager(IEnqueue<ScanResult> targetConsumer, TimeSpan samplingInterval, string caseUrl) : base(targetConsumer, samplingInterval, caseUrl)
        {
            _channelBaseString = "Com";
            _serialPorts = new List<SerialPort>();
            _serialReaderList = new List<SerialReader>();
            _serialWriterList = new List<SerialWriter>();

            //_deviceSessionList = new List<Session>();
            _attachedConverter = StaticMethods.CreateConverterInstance(DeviceName);
        }

        //internal SL508Input SerialInput { get => _serialInput; set => _serialInput = value; }
        //internal SL508OutputDeviceManager SerialOutput { get => _serialOutput; set => _serialOutput = value; }

        public override string GetFormattedStatus()
        {
            string formattedString = "";
            if (null != _lastMessage)
            {
                int l = (_lastMessage.Length > 20) ? 20 : _lastMessage.Length;
                System.Diagnostics.Debug.Assert(l > 0);
                formattedString = "First bytes: " + BitConverter.ToString(_lastMessage).Substring(0, l*3-1);
            }
            return formattedString;
        }

        AsyncCallback readerAsyncCallback;
        private IAsyncResult readerIAsyncResult;
        private bool OpenDevices(string baseUrl, string deviceName)
        {
            _deviceSession = new Session();

            var n = _deviceSession.GetNumberOfChannels();

            foreach(var channel in Config.Instance.SerialChannels)
            {
                string finalUrl = baseUrl + channel.portname.ToString();
                var port = _deviceSession.CreateSerialPort(finalUrl,
                                    channel.mode,
                                    channel.baudrate,
                                    SerialPortDataBits.DataBits8,
                                    SerialPortParity.None,
                                    SerialPortStopBits.StopBits1,
                                    "\n");
                _serialPorts.Add(port);
            }

#if old
            for (int ch = 0; ch < 8; ch++)
            {
                string finalUrl = baseUrl + _channelBaseString + ch.ToString();

                _deviceSession.CreateSerialPort(finalUrl,
                                    SerialPortMode.RS232,
                                    SerialPortSpeed.BitsPerSecond9600,
                                    SerialPortDataBits.DataBits8,
                                    SerialPortParity.None,
                                    SerialPortStopBits.StopBits1,
                                    "\n");
            }
#endif
            _deviceSession.ConfigureTimingForMessagingIO(100, 100.0);
            _deviceSession.GetTiming().SetTimeout(500); // timeout to throw from _serialReader.EndRead (looks like default is 1000)
            _deviceSession.Start();

            readerAsyncCallback = new AsyncCallback(ReaderCallback);

            bool firstIteration = true;
            foreach(SerialPort port in _serialPorts)
            {
                int channleIndex = port.GetIndex();
                if (firstIteration)
                {
                    firstIteration = false;
                    _logger.Info($"*** {DeviceName} init:");
                        //port.GetResourceName());
                }
               
                _logger.Info($"Serial CH{channleIndex} init success. {port.GetMode()}  {port.GetSpeed()}.");////{c111.GetResourceName()}");

                SerialReader sr = new SerialReader(_deviceSession.GetDataStream(), channleIndex);
                readerIAsyncResult = sr.BeginRead(200, readerAsyncCallback, channleIndex);
                _serialReaderList.Add(sr);
                SerialWriter sw = new SerialWriter(_deviceSession.GetDataStream(), channleIndex);
                _serialWriterList.Add(sw);

            }

#if old
            for (int ch = 0; ch < 8; ch++)
            {
                //var c111 = _deviceSession.GetChannel(ch);

                _logger.Info($"{DeviceName} Serial CH{ch} init success (input/output): 9600bps ");////{c111.GetResourceName()}");

                SerialReader sr = new SerialReader(_deviceSession.GetDataStream(), _deviceSession.GetChannel(ch).GetIndex());
                readerIAsyncResult = sr.BeginRead(200, readerAsyncCallback, ch);
                _serialReaderList.Add(sr);
                SerialWriter sw = new SerialWriter(_deviceSession.GetDataStream(), _deviceSession.GetChannel(ch).GetIndex());
                _serialWriterList.Add(sw);
            }
#endif
            return true;

        }

        public void ReaderCallback(IAsyncResult ar)
        {
            int channel = (int)ar.AsyncState;
            if (null == _serialReaderList[channel]) // if during dispose
            {
                return;
            }
            try
            {
                byte[] receiveBuffer = _serialReaderList[channel].EndRead(ar);
                //byte[] receiveBuffer = { 1, 2, 3 };

                _lastMessage = receiveBuffer;
                // send reply (debug only)
                string str = System.Text.Encoding.ASCII.GetString(receiveBuffer);
                byte[] reply = System.Text.Encoding.ASCII.GetBytes("Reply> " + str);
                _serialWriterList[channel].Write(reply);
                //_logger.Debug(str);

                // forward to consumer (send by udp)
                ScanResult sr = new ScanResult(receiveBuffer, this);
                _targetConsumer.Enqueue(sr);

                // restart reader
                if (_serialReaderList[channel] != null && base._deviceSession.IsRunning() && _InDisposeState == false)
                {
                    readerIAsyncResult = _serialReaderList[channel].BeginRead(200, this.ReaderCallback, channel);
                }
            }
            catch (UeiDaqException ex)
            {
                // only handle exception if the session is running
                if (null != base._deviceSession)
                {
                    if (base._deviceSession.IsRunning())
                    {
                        if (Error.Timeout == ex.Error)
                        {
                            // Ignore timeout error, they will occur if the send button is not
                            // clicked on fast enough!
                            if (false == _InDisposeState)
                            {
                                readerIAsyncResult = _serialReaderList[channel].BeginRead(200, readerAsyncCallback, channel);
                            }
                        }
                        else
                        {
                            //base._deviceSession.Dispose();
                            //base._deviceSession = null;
                            Console.WriteLine(ex.Message);
                        }
                    }
                }
            }
            //catch (NullReferenceException ex)
            //{
            //    _logger.Warn( ex.Message);
            //}


        }
        public override void Start()
        {
            // init session upon need
            // =======================
            string deviceIndex = StaticMethods.FindDeviceIndex(DeviceName);
            if (null == deviceIndex)
            {
                _logger.Warn($"Can't find index for device {DeviceName}");
                return;
            }

            string url1 = _caseUrl + deviceIndex + _channelsString;

            if (OpenDevices(url1, DeviceName))
            {
                //_logger.Info($"{DeviceName}(Input/Output) init success. {_deviceSessionList[0].GetNumberOfChannels()} channels.  {deviceIndex + _channelsString}");

            }
            else
            {
                _logger.Warn($"Device {DeviceName} init fail");
                return;
            }
#if dont
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    try
                    {
                        byte[] b = _serialReader.Read(10);
                        System.Diagnostics.Debug.Assert(b == null);
                    }
                    catch (UeiDaqException ex)
                    {
                        if (Error.Timeout != ex.Error)
                        {

                            System.Diagnostics.Debug.Assert(false);
                        }
                    }
                    System.Threading.Thread.Sleep(10);
                }
            }
            );
#endif
        }

        public override void Dispose()
        {
            _InDisposeState = true;
            System.Threading.Thread.Sleep(500);
            CloseDevices();
        }

        private void CloseDevices()
        {
            for (int i = 0; i < _serialReaderList.Count; i++)
            {
                _serialReaderList[i].Dispose();
                _serialReaderList[i] = null;
            }
            for (int i = 0; i < _serialWriterList.Count; i++)
            {
                _serialWriterList[i].Dispose();
                _serialWriterList[i] = null;
            }

            _deviceSession.Stop();
            _deviceSession.Dispose();
            _deviceSession = null;

        }
    }
}
