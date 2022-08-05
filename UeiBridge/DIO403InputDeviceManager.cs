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
        log4net.ILog _logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);
        

        readonly IEnqueue<DeviceResponse> _targetConsumer;

        public DIO403InputDeviceManager(IEnqueue<DeviceResponse> targetConsumer, TimeSpan samplingInterval, string caseUrl)
        {
            _targetConsumer = targetConsumer;
            _samplingInterval = samplingInterval;
            _deviceName = "DIO-403";
            _caseUrl = caseUrl;
            _channelsString = "Di3:5";
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
        public void HandleResponse_Callback(object state)
        {
            lock (this) // tbd. use q
            {
                // init session, if needed.
                // =======================
                if (null == _deviceSession)
                {
                    CloseDevice();

                    string deviceIndex = StaticMethods.FindDeviceIndex(_deviceName);
                    if (null == deviceIndex)
                    {
                        _logger.Warn($"Can't find index for device {_deviceName}");
                        return;
                    }
                    string url1 = _caseUrl + deviceIndex + _channelsString;

                    if (OpenDevice(url1))
                    {
                        //_logger.Info($"{_deviceName} init success. {_numberOfChannels} input channels. {url1}");
                        _logger.Info($"{_deviceName}(Input) init success. {_numberOfChannels} channels. {deviceIndex + _channelsString}");
                    }
                    else
                    {
                        _logger.Warn($"Device {_deviceName} init fail");
                        return;
                    }
                }
            }
            // read from device
            // ===============
            var diData = _reader.ReadSingleScanUInt16();
            System.Diagnostics.Debug.Assert(diData != null);
            System.Diagnostics.Debug.Assert(diData.Length == _numberOfChannels, "wrong number of channels");

            DeviceResponse dr = new DeviceResponse(diData, _deviceName);
            _targetConsumer.Enqueue(dr);

        }
        public void Start()
        {
            _samplingTimer = new System.Threading.Timer(HandleResponse_Callback, null, TimeSpan.Zero, _samplingInterval);
        }

    }
}
