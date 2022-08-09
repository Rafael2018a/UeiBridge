using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UeiDaq;

namespace UeiBridge
{
    /// <summary>
    /// ** from the manual:
    /// ** Sequential Sampling, 16-bit, 24-channel Analog Input board
    /// </summary>
    class AI201InputDeviceManager : InputDevice
    {
        AnalogScaledReader _reader;
        log4net.ILog _logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);
        readonly IEnqueue<ScanResult> _targetConsumer;

        public AI201InputDeviceManager( IEnqueue<ScanResult> targetConsumer, TimeSpan samplingInterval, string caseUrl)
        {
            _targetConsumer = targetConsumer;
            _samplingInterval = samplingInterval;
            _deviceName = "AI-201-100";
            _caseUrl = caseUrl;
            _channelsString = "Ai0,1,2,3";
            _attachedConverter = StaticMethods.CreateConverterInstance(_deviceName);
        }
        bool OpenDevice(string deviceUrl)
        {
            try
            {
                _deviceSession = new Session();
                var x = Config.Instance.AnalogInMinMaxVoltage;
                _deviceSession.CreateAIChannel(deviceUrl, -5, 5, AIChannelInputMode.SingleEnded);
                _numberOfChannels = _deviceSession.GetNumberOfChannels();
                _deviceSession.ConfigureTimingForSimpleIO();
                _reader = new AnalogScaledReader(_deviceSession.GetDataStream());
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
                    var r = _deviceSession.GetDevice().GetAIRanges();
                    _logger.Info($"{_deviceName}(input) init success. {_numberOfChannels} channels. Range {r[0].minimum},{r[0].maximum}. {deviceIndex + _channelsString}");
                }
                else
                {
                    _logger.Warn($"Device {_deviceName} init fail");
                    return;
                }
            }

            // read from device
            // ===============
            var aiData = _reader.ReadSingleScan();
            System.Diagnostics.Debug.Assert(aiData != null);
            System.Diagnostics.Debug.Assert(aiData.Length == _numberOfChannels, "wrong number of channels");

            ScanResult dr = new ScanResult(aiData, this);
            _targetConsumer.Enqueue(dr);
        }

        public void Start()
        {
            _samplingTimer = new System.Threading.Timer(HandleResponse_Callback, null, TimeSpan.Zero, _samplingInterval);
        }

    }
}
