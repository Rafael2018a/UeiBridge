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
        readonly List<SerialReader> _serialReaderList;
        readonly List<SerialWriter> _serialWriterList;
        IConvert _attachedConverter;
        //private SerialReader _serialReader;
        //private SerialWriter _serialWriter;

        //readonly List<Session> _deviceSessionList;
        string _channelBaseString;

        public override IConvert AttachedConverter => _attachedConverter;

        public List<SerialWriter> SerialWriterList => _serialWriterList;

        public SL508InputDeviceManager(IEnqueue<ScanResult> targetConsumer, TimeSpan samplingInterval, string caseUrl) : base(targetConsumer, samplingInterval, caseUrl)
        {
            _channelBaseString = "Com";
            _serialReaderList = new List<SerialReader>();
            _serialWriterList = new List<SerialWriter>();
            //_deviceSessionList = new List<Session>();
            _attachedConverter = StaticMethods.CreateConverterInstance(DeviceName);
        }

        //internal SL508Input SerialInput { get => _serialInput; set => _serialInput = value; }
        //internal SL508OutputDeviceManager SerialOutput { get => _serialOutput; set => _serialOutput = value; }

        public override string GetFormattedStatus()
        {
            return "SL-508-892 input handler not ready yet";
        }

        AsyncCallback readerAsyncCallback;
        private IAsyncResult readerIAsyncResult;
        private bool OpenDevices(string baseUrl, string deviceName)
        {
            _deviceSession = new Session();

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

            _deviceSession.ConfigureTimingForMessagingIO(100, 100.0);
            _deviceSession.GetTiming().SetTimeout(500); // timeout to throw from _serialReader.EndRead (looks like default is 1000)
            _deviceSession.Start();

            readerAsyncCallback = new AsyncCallback(ReaderCallback);

            for (int ch = 0; ch < 8; ch++)
            {
                var c111 = _deviceSession.GetChannel(ch);

                _logger.Info($"serial port {ch}: {c111.GetResourceName()}");
                //var r = c111.GetResourceName();
               

                SerialReader sr = new SerialReader(_deviceSession.GetDataStream(), _deviceSession.GetChannel(ch).GetIndex());
                readerIAsyncResult = sr.BeginRead(200, readerAsyncCallback, ch);
                _serialReaderList.Add( sr);
                SerialWriter sw = new SerialWriter(_deviceSession.GetDataStream(), _deviceSession.GetChannel(ch).GetIndex());
                _serialWriterList.Add(sw);
            }


                return true;
            ////////////////////////////////////////////////////////////////////////////

#if dont


            _deviceSession = new UeiDaq.Session();
            SerialPort sp =
            _deviceSession.CreateSerialPort(url,
                                           SerialPortMode.RS232,
                                           SerialPortSpeed.BitsPerSecond9600,
                                           SerialPortDataBits.DataBits8,
                                           SerialPortParity.None,
                                           SerialPortStopBits.StopBits1,
                                           "\n");

            _deviceSession.ConfigureTimingForMessagingIO(100, 100.0);
            //_deviceSession.GetTiming().SetTimeout(500); // timeout to throw from _serialReader.EndRead (looks like default is 1000)

            _deviceSession.Start();

            _serialReader = new SerialReader(_deviceSession.GetDataStream(), _deviceSession.GetChannel(0).GetIndex());
            _serialWriter = new SerialWriter(_deviceSession.GetDataStream(), _deviceSession.GetChannel(0).GetIndex());

            readerAsyncCallback = new AsyncCallback(ReaderCallback);
            readerIAsyncResult = _serialReader.BeginRead(200, readerAsyncCallback, null);

            return true;
#endif
        }

        public void ReaderCallback(IAsyncResult ar)
        {
            int channel = (int)ar.AsyncState;
            try
            {
                
                byte[] receiveBuffer = _serialReaderList[channel].EndRead(ar);

                // We can't directly access the UI from an asynchronous method
                // need to invoke a delegate that will take care of updating
                // the UI from the proper thread
                string str = System.Text.Encoding.ASCII.GetString(receiveBuffer);
                byte[] reply = System.Text.Encoding.ASCII.GetBytes("Reply> " + str);
                _serialWriterList[channel].Write(reply);
                _logger.Debug(str);
                if (_serialReaderList[channel] != null && base._deviceSession.IsRunning())
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
                            // Just reinitiate a new asynchronous read.
                            readerIAsyncResult = _serialReaderList[channel].BeginRead(200, readerAsyncCallback, channel);
                            //_logger.Debug("Timeout");
                        }
                        else
                        {
                            base._deviceSession.Dispose();
                            base._deviceSession = null;
                            Console.WriteLine(ex.Message);
                        }
                    }
                }
            }


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
