using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        //byte[] _lastMessage;
        List<byte[]> _lastMessagesList;
        //readonly List<Session> _deviceSessionList;
        string _channelBaseString;

        public override IConvert AttachedConverter => _attachedConverter;
        public List<SerialWriter> SerialWriterList => _serialWriterList;
        public bool InDisposeState => _InDisposeState;

        public bool IsReady => _isReady; 

        //IEnqueue<ScanResult> _targetConsumer;
        bool _isReady = false;

        public SL508InputDeviceManager(IEnqueue<ScanResult> targetConsumer, TimeSpan samplingInterval, string caseUrl) : base(targetConsumer, samplingInterval, caseUrl)
        {
            _channelBaseString = "Com";
            _serialPorts = new List<SerialPort>();
            _serialReaderList = new List<SerialReader>();
            _serialWriterList = new List<SerialWriter>();

            //_deviceSessionList = new List<Session>();
            _attachedConverter = StaticMethods.CreateConverterInstance(DeviceName);

            _lastMessagesList = new List<byte[]>();
            for (int i = 0; i < 8; i++)
            {
                _lastMessagesList.Add(null);
            }

        }

        //internal SL508Input SerialInput { get => _serialInput; set => _serialInput = value; }
        //internal SL508OutputDeviceManager SerialOutput { get => _serialOutput; set => _serialOutput = value; }

        //public override string GetFormattedStatus()
        //{
        //    string formattedString = "";
        //    if (null != _lastMessage)
        //    {
        //        int l = (_lastMessage.Length > 20) ? 20 : _lastMessage.Length;
        //        System.Diagnostics.Debug.Assert(l > 0);
        //        formattedString = "First bytes: " + BitConverter.ToString(_lastMessage).Substring(0, l*3-1);
        //    }
        //    return formattedString;
        //}

        public override string GetFormattedStatus()
        {
            StringBuilder formattedString = new StringBuilder();
            for (int ch = 0; ch < 8; ch++)
            {
                byte[] last = _lastMessagesList[ch];
                if (null != last)
                {
                    int len = (last.Length > 20) ? 20 : last.Length;
                    string s = $"Payload (in) ch{ch}: {BitConverter.ToString(last).Substring(0, len * 3 - 1)}\n";
                    formattedString.Append(s);
                }
            }
            return formattedString.ToString();
        }
#if asyncSerial
        /// <summary>
        /// form 'SerialLoopbackAsync' example
        /// </summary>
        private bool OpenDevices(string baseUrl, string deviceName)
        {
            // amount of data needed to trigger event
            int watermark = 105;
            // time without more data to trigger event
            // allows reading data leftover that is less than WATERMARK level
            // set to 0 to disable
            int timeout_us = 1000000;

            _deviceSession = new Session();
            //"pdna://192.168.100.2/Dev3/com0,1"
            string finalUrl = baseUrl + "com0,1";
            SerialPort port = _deviceSession.CreateSerialPort( finalUrl,
                SerialPortMode.RS232,
                SerialPortSpeed.BitsPerSecond9600,
                SerialPortDataBits.DataBits8,
                SerialPortParity.None,
                SerialPortStopBits.StopBits1,
                "");

            _deviceSession.ConfigureTimingForAsynchronousIO( watermark, 0, timeout_us, 0);
            _deviceSession.GetTiming().SetTimeout(10);

            for (int i = 0; i < _deviceSession.GetNumberOfChannels(); i++)
            {
                int chanNum = _deviceSession.GetChannel(i).GetIndex();

                _serialWriterList.Add( new SerialWriter( _deviceSession.GetDataStream(), chanNum));
                _serialReaderList.Add(new SerialReader(_deviceSession.GetDataStream(), chanNum));
               
            }

            _deviceSession.Start();

            return true;
        }
#endif
        AsyncCallback readerAsyncCallback;
        private IAsyncResult readerIAsyncResult;
        int minLen = 200;
        private bool OpenDevices(string baseUrl, string deviceName)
        {
            _deviceSession = new Session();

            foreach(var channel in Config.Instance.SerialChannels)
            {
                                string finalUrl = baseUrl + channel.portname.ToString();
                //string finalUrl = baseUrl + "Com0,1";
                var port = _deviceSession.CreateSerialPort(finalUrl,
                                    SerialPortMode.RS232,
                                    SerialPortSpeed.BitsPerSecond9600,
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

            for (int ch = 0; ch < numberOfChannels; ch++)
            {
                SerialReader sr = new SerialReader(_deviceSession.GetDataStream(), _deviceSession.GetChannel(ch).GetIndex());
                _serialReaderList.Add(sr);
                SerialWriter sw = new SerialWriter(_deviceSession.GetDataStream(), _deviceSession.GetChannel(ch).GetIndex());
                _serialWriterList.Add(sw);
                
            }
            _deviceSession.Start();

            for (int ch = 0; ch < numberOfChannels; ch++)
            {
                readerAsyncCallback = new AsyncCallback(ReaderCallback);
                readerIAsyncResult = _serialReaderList[ch].BeginRead(minLen, readerAsyncCallback, ch);
                
            }

            bool firstIteration = true;
            foreach (SerialPort port in _serialPorts)
            {
                int channleIndex = port.GetIndex();
                if (firstIteration)
                {
                    firstIteration = false;
                    _logger.Info($"*** {DeviceName} init:");
                    //port.GetResourceName());
                }

                _logger.Info($"Serial CH{channleIndex} init success. {port.GetMode()}  {port.GetSpeed()}.");////{c111.GetResourceName()}");


            }

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

                //_logger.Debug($"Received ch{channel} {BitConverter.ToString( receiveBuffer)}");
                //byte[] receiveBuffer = { 1, 2, 3 };

                _lastMessagesList[channel] = receiveBuffer;
                // send reply (debug only)
                //string str = System.Text.Encoding.ASCII.GetString(receiveBuffer);
                //byte[] reply = System.Text.Encoding.ASCII.GetBytes("Reply> " + str);
                //_serialWriterList[channel].Write(reply);
                //_logger.Debug(str);

                // forward to consumer (send by udp)
                ScanResult sr = new ScanResult(receiveBuffer, this);
                _targetConsumer.Enqueue(sr);


                // restart reader
                if (_serialReaderList[channel] != null && base._deviceSession.IsRunning() && _InDisposeState == false)
                {
                    readerIAsyncResult = _serialReaderList[channel].BeginRead(minLen, this.ReaderCallback, channel);
                }
            }
            catch (UeiDaqException ex)
            {
                // only handle exception if the session is running
                if (null != base._deviceSession)
                {
                    if (base._deviceSession.IsRunning())
                    {
                        _lastMessagesList[channel] = null;

                        if (Error.Timeout == ex.Error)
                        {
                            // Ignore timeout error, they will occur if the send button is not
                            // clicked on fast enough!
                            if (false == _InDisposeState)
                            {
                                readerIAsyncResult = _serialReaderList[channel].BeginRead(minLen, readerAsyncCallback, channel);
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
                _isReady = true;
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
#if asyncSerial
        void ReadChannelsTask()
        {
            // do read for each channel
            for (int i = 0; i < _deviceSession.GetNumberOfChannels(); i++)
            {
                int chanNum = _deviceSession.GetChannel(i).GetIndex();
                Console.WriteLine("reader[{0}]: ", i);

                // "available" is the amount of data remaining from a single event
                // more data may be in the queue once "available" bytes have been read
                int available;
                while ((available = _deviceSession.GetDataStream().GetAvailableInputMessages(chanNum)) > 0 )
                {
                    Console.Write("  avail: {0}", _deviceSession.GetDataStream().GetAvailableInputMessages(chanNum));
                    try
                    {
                        byte [] reply = _serialReaderList[chanNum].ReadTimestamped(4);
                        _logger.Info($"reply {reply.Length}");
                    }
                    catch (UeiDaqException e)
                    {
                        if (e.Error == Error.Timeout)
                        {
                            Console.WriteLine("  read timeout");
                            break;
                        }
                        else
                        {
                            throw e;
                        }
                    }
                }
            }

        }
#endif
        public override void Dispose()
        {
            _InDisposeState = true;
            System.Threading.Thread.Sleep(500);
            CloseDevices();
        }

        private void CloseDevices()
        {
            _deviceSession.Stop();

            for (int i = 0; i < _serialReaderList.Count; i++)
            {
                _serialReaderList[i].Dispose();
                _serialReaderList[i] = null;
            }
            for (int i = 0; i < _serialWriterList.Count; i++)
            {
                if (null != _serialWriterList[i])
                {
                    _serialWriterList[i].Dispose();
                    _serialWriterList[i] = null;
                }
            }

            _deviceSession.Dispose();
            _deviceSession = null;

        }
    }
}
