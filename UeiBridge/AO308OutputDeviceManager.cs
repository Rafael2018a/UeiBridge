using System;
using UeiDaq;

namespace UeiBridge
{
    /// <summary>
    /// from the manual:
    /// ** 8-Channel, 16-bit, ±10V Analog Output Board **
    /// </summary>
    internal class AO308OutputDeviceManager : OutputDevice
    {
        AnalogScaledWriter _writer;
        log4net.ILog _logger = log4net.LogManager.GetLogger("Root");

        public AO308OutputDeviceManager()
        {
            _deviceName = "AO-308";
            _channelsString = "Ao0:7";
            
            _attachedConverter = StaticMethods.CreateConverterInstance(_deviceName);
        }

        // todo: add Dispose/d-tor
        bool OpenDevice(string deviceUrl)
        {
            try
            {
                _deviceSession = new Session();
                var minmax = Config.Instance.Analog_Out_MinMaxVoltage;
                _deviceSession.CreateAOChannel(deviceUrl, minmax.Item1, minmax.Item2);
                _numberOfChannels = _deviceSession.GetNumberOfChannels();
                _deviceSession.ConfigureTimingForSimpleIO();
                _writer = new AnalogScaledWriter(_deviceSession.GetDataStream());
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                _deviceSession = null;
                return false;
            }
            return true;
        }

        protected override void HandleRequest(DeviceRequest dr)
        {
            // init session, if needed.
            // =======================
            if ((null == _deviceSession) || (_caseUrl != dr.CaseUrl))
            {
                CloseDevice();

                string deviceIndex = StaticMethods.FindDeviceIndex(_deviceName);
                if (null == deviceIndex)
                {
                    _logger.Warn($"Can't find index for device {_deviceName}");
                    return;
                }

                string url1 = dr.CaseUrl + deviceIndex + _channelsString;

                if (OpenDevice(url1))
                {
                    var range = _deviceSession.GetDevice().GetAORanges();

                    //_logger.Info($"{_deviceName} init success. {_numberOfChannels} output channels. {url1}");
                    _logger.Info($"{_deviceName}(Output) init success. {_numberOfChannels} channels. Range {range[0].minimum},{range[0].maximum}. {deviceIndex + _channelsString}");
                    _caseUrl = dr.CaseUrl;
                }
                else
                {
                    _logger.Warn($"Device {_deviceName} init fail");
                    return;
                }
            }

            // write to device
            // ===============
            double[] req = dr.RequestObject as double[];
            if (null != req)
            {
                _writer.WriteSingleScan(req);
                _logger.Debug($"AO voltage {req[0]}");
                _logger.Debug($"scan written to device. Length: {req.Length}");
            }
        }
    }
}