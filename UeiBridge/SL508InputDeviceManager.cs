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
        log4net.ILog _logger = log4net.LogManager.GetLogger("Root");
        //SL508Input _serialInput;
        //SL508OutputDeviceManager _serialOutput;
        public override string DeviceName => "SL-508-892";
        //readonly new string _channelsString;
        private SerialReader _serialReader;
        private SerialWriter _serialWriter;

        //Session _deviceSession;

        public override IConvert AttachedConverter => throw new NotImplementedException();

        public SL508InputDeviceManager(IEnqueue<ScanResult> targetConsumer, TimeSpan samplingInterval, string caseUrl) : base(targetConsumer, samplingInterval, caseUrl)
        {
            _channelsString = "Com0,1";
        }

        //internal SL508Input SerialInput { get => _serialInput; set => _serialInput = value; }
        //internal SL508OutputDeviceManager SerialOutput { get => _serialOutput; set => _serialOutput = value; }

        public override string GetFormattedStatus()
        {
            return "SL-508-892 input handler not ready yet";
        }

        AsyncCallback readerAsyncCallback;
        private IAsyncResult readerIAsyncResult;
        private bool OpenDevice(string url, string deviceName)
        {
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
            _deviceSession.GetTiming().SetTimeout(500);

            _deviceSession.Start();

            _serialReader = new SerialReader(_deviceSession.GetDataStream(), _deviceSession.GetChannel(0).GetIndex());

            readerAsyncCallback = new AsyncCallback(ReaderCallback);
            readerIAsyncResult = _serialReader.BeginRead(200, readerAsyncCallback, null);

            return true;

        }

        public void ReaderCallback(IAsyncResult ar)
        {
            try
            {
                byte[] recvBytes = _serialReader.EndRead(ar);

                // We can't directly access the UI from an asynchronous method
                // need to invoke a delegate that will take care of updating
                // the UI from the proper thread

                if (_serialReader != null && _deviceSession.IsRunning())
                {
                    readerIAsyncResult = _serialReader.BeginRead(200, ReaderCallback, null);
                }
            }
            catch (UeiDaqException ex)
            {
                // only handle exception if the session is running
                if (null != _deviceSession)
                {
                    if (_deviceSession.IsRunning())
                    {
                        if (Error.Timeout == ex.Error)
                        {
                            // Ignore timeout error, they will occur if the send button is not
                            // clicked on fast enough!
                            // Just reinitiate a new asynchronous read.
                            readerIAsyncResult = _serialReader.BeginRead(200, readerAsyncCallback, null);
                            //Console.WriteLine("Timeout");
                        }
                        else
                        {
                            _deviceSession.Dispose();
                            _deviceSession = null;
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

            if (OpenDevice(url1, DeviceName))
            {
                _logger.Info($"{DeviceName}(Input/Output) init success. {_deviceSession.GetNumberOfChannels()} channels.  {deviceIndex + _channelsString}");

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
            CloseDevice();
        }
    }
}
