using System;
using UeiDaq;

namespace UeiBridge
{
    /// <summary>
    /// from the manual:
    /// ** 48-channel Digital I/O **
    /// </summary>
    class DIO403InputDeviceManager : InputDevice
    {
        DigitalReader _reader;
        log4net.ILog _logger = log4net.LogManager.GetLogger("Root");
        public override string DeviceName => "DIO-403";
        IConvert _attachedConverter;
        public override IConvert AttachedConverter => _attachedConverter;

        public DIO403InputDeviceManager(IEnqueue<ScanResult> targetConsumer, TimeSpan samplingInterval, string caseUrl): base( targetConsumer, samplingInterval, caseUrl)
        {
            _channelsString = "Di3:5";
            _attachedConverter = StaticMethods.CreateConverterInstance(DeviceName);
        }
        // todo: add Dispose/d-tor
        bool OpenDevice(string deviceUrl)
        {
            try
            {
                _deviceSession = new Session();
                _deviceSession.CreateDIChannel(deviceUrl);
                _numberOfChannels = _deviceSession.GetNumberOfChannels();
                _deviceSession.ConfigureTimingForSimpleIO();
                _reader = new DigitalReader(_deviceSession.GetDataStream());
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                _deviceSession = null;
                return false;
            }

            return true;
        }
        public void DeviceScan_Callback(object state)
        {
            if (null == _deviceSession)
            {
                lock (this)
                {
                    // init session, if needed.
                    // =======================
                    if (null == _deviceSession)
                    {
                        CloseDevice();

                        string deviceIndex = StaticMethods.FindDeviceIndex(DeviceName);
                        if (null == deviceIndex)
                        {
                            _logger.Warn($"Can't find index for device {DeviceName}");
                            return;
                        }
                        string url1 = _caseUrl + deviceIndex + _channelsString;

                        if (OpenDevice(url1))
                        {
                            //_logger.Info($"{_deviceName} init success. {_numberOfChannels} input channels. {url1}");
                            _logger.Info($"{DeviceName}(Input) init success. {_numberOfChannels} channels. {deviceIndex + _channelsString}");
                        }
                        else
                        {
                            _logger.Warn($"Device {DeviceName} init fail");
                            return;
                        }
                    }
                }
            }
            // read from device
            // ===============
            _lastScan = _reader.ReadSingleScanUInt16();
            System.Diagnostics.Debug.Assert(_lastScan != null);
            System.Diagnostics.Debug.Assert(_lastScan.Length == _numberOfChannels, "wrong number of channels");
            //diData[0] = 0x07;
            ScanResult dr = new ScanResult(_lastScan, this);
            _targetConsumer.Enqueue(dr);

        }
        public override void Start()
        {
            _samplingTimer = new System.Threading.Timer(DeviceScan_Callback, null, TimeSpan.Zero, _samplingInterval);
        }
        UInt16 [] _lastScan;
        public override string GetFormattedStatus()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder("Input bits: ");
            if (null != _lastScan)
            {
                foreach (UInt16 val in _lastScan)
                {
                    sb.Append(Convert.ToString(val, 2).PadLeft(8, '0'));
                    sb.Append("  ");
                }
            }
            return sb.ToString();
        }

    }
}
