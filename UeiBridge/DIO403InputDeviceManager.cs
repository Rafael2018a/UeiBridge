using System;
using UeiDaq;
using UeiBridgeTypes;


namespace UeiBridge
{
    /// <summary>
    /// from the manual:
    /// ** 48-channel Digital I/O **
    /// </summary>
    class DIO403InputDeviceManager : InputDevice
    {
        DigitalReader _reader;
        log4net.ILog _logger = StaticMethods.GetLogger();
        public override string DeviceName => "DIO-403";
        IConvert _attachedConverter;
        public override IConvert AttachedConverter => _attachedConverter;

        public override string InstanceName => _instanceName; 

        string _instanceName;
        //public DIO403InputDeviceManager(IEnqueue<ScanResult> targetConsumer, TimeSpan samplingInterval, string caseUrl) : base(targetConsumer, samplingInterval, caseUrl)
        //{
        //    _channelsString = "Di3:5";
        //    _attachedConverter = StaticMethods.CreateConverterInstance(DeviceName);
        //}

        public DIO403InputDeviceManager( ISend<SendObject> targetConsumer, DeviceSetup setup): base(targetConsumer, setup)
        {
            _channelsString = "Di3:5";
            _attachedConverter = StaticMethods.CreateConverterInstance(DeviceName);
            _instanceName = $"{DeviceName}/{setup.SlotNumber}";
        }
        public DIO403InputDeviceManager():base(null, null) // must have default const.
        {

        }
        // todo: add Dispose/d-tor
        bool OpenDevice(string deviceUrl)
        {
            try
            {
                _deviceSession = new Session();
                _deviceSession.CreateDIChannel(deviceUrl);
                //_numberOfChannels = _deviceSession.GetNumberOfChannels();
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
            // read from device
            // ===============
            try
            {
                _lastScan = _reader.ReadSingleScanUInt16();
                System.Diagnostics.Debug.Assert(_lastScan != null);
                System.Diagnostics.Debug.Assert(_lastScan.Length == _deviceSession.GetNumberOfChannels(), "wrong number of channels");
                //diData[0] = 0x07;
                ScanResult dr = new ScanResult(_lastScan, this);
                //_targetConsumer.Enqueue(dr); tbd
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
            }
        }
        public override void OpenDevice()
        {
            _logger.Debug("DIO304 input, opendevice .... tbd");
            return;

            if ((_deviceSession != null) && _deviceSession.IsRunning())
            {
                _logger.Warn("Can't start since device already running");
                return;
            }

            // init session, if needed.
            // =======================
            string deviceIndex = StaticMethods.FindDeviceIndex( _cubeUrl, DeviceName);
            if (null == deviceIndex)
            {
                _logger.Warn($"Can't find index for device {DeviceName}");
                return;
            }
            string deviceUrl = _cubeUrl + deviceIndex + _channelsString;

            if (OpenDevice(deviceUrl))
            {
                _logger.Info($"{DeviceName}(Input) init success. {_deviceSession.GetNumberOfChannels()} channels. {deviceIndex + _channelsString}");
            }
            else
            {
                _logger.Warn($"Device {DeviceName} init fail");
                return;
            }

            _samplingTimer = new System.Threading.Timer(DeviceScan_Callback, null, TimeSpan.Zero, _samplingInterval);
        }
        public override void Dispose()
        {
            _samplingTimer.Dispose();
            System.Threading.Thread.Sleep(200); // wait for callback to finish
            CloseDevice();
        }

        UInt16[] _lastScan;
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
