using System;
using UeiDaq;

namespace UeiBridge
{
    abstract class DioOutputDeviceManager : OutputDevice
    {
        log4net.ILog _logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);
        protected DigitalWriter _writer;

        protected bool OpenDevice( DeviceRequest dr)
        {
            string deviceIndex = StaticMethods.FindDeviceIndex(_deviceName);
            if (null == deviceIndex)
            {
                _logger.Warn($"Can't find index for device {_deviceName}");
                return false;
            }

            string deviceUrl = dr.CaseUrl + deviceIndex + _channelsString;

            if (CreateDigitalSession(deviceUrl))
            {
                _logger.Info($"{_deviceName}(Output) init success. {_numberOfChannels} channels. {deviceIndex + _channelsString}");
                _caseUrl = dr.CaseUrl;
            }
            else
            {
                _logger.Warn($"Device {_deviceName} init fail");
                return false;
            }
            return true;
        }

        public override void HandleRequest(DeviceRequest dr)
        {
            // init session, if needed.
            // =======================
            if ((null == _deviceSession) || (_caseUrl != dr.CaseUrl))
            {
                CloseDevice(); // if needed
                OpenDevice(dr);
            }

            // write to device
            // ===============
            UInt16[] req = dr.RequestObject as UInt16[];
            System.Diagnostics.Debug.Assert((null != req));
            
            _writer.WriteSingleScanUInt16(req);
            
        }

        // todo: add Dispose/d-tor

        bool CreateDigitalSession(string deviceUrl)
        {
            try
            {
                _deviceSession = new Session();
                _deviceSession.CreateDOChannel(deviceUrl);
                _numberOfChannels = _deviceSession.GetNumberOfChannels();
                _deviceSession.ConfigureTimingForSimpleIO();
                _writer = new DigitalWriter(_deviceSession.GetDataStream());
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                _deviceSession = null;
                return false;
            }
            return true;
        }
    }
}

