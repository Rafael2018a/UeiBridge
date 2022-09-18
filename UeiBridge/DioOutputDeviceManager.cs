using System;
using UeiDaq;

namespace UeiBridge
{
    abstract class DioOutputDeviceManager : OutputDevice
    {
        log4net.ILog _logger = log4net.LogManager.GetLogger("Root");
        protected DigitalWriter _writer;
        

        protected bool OpenDevice( DeviceRequest dr, string _deviceName)
        {
            
            string deviceIndex = StaticMethods.FindDeviceIndex(_deviceName);
            if (null == deviceIndex)
            {
                _logger.Warn($"Can't find index for device {_deviceName}");
                return false;
            }

            string deviceUrl = dr.CaseUrl + deviceIndex + ChannelsString;

            if (CreateDigitalSession(deviceUrl))
            {
                _logger.Info($"{_deviceName}(Output) init success. {_deviceSession.GetNumberOfChannels()} channels. {deviceIndex + ChannelsString}");
                _caseUrl = dr.CaseUrl;
            }
            else
            {
                _logger.Warn($"Device {_deviceName} init fail");
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
                CloseDevice(); // if needed
                OpenDevice(dr, DeviceName);
            }

            // write to device
            // ===============
            UInt16[] req = dr.RequestObject as UInt16[];
            System.Diagnostics.Debug.Assert((null != req));
            
            _writer.WriteSingleScanUInt16(req);
            _lastScan = req;
            _logger.Debug($"scan written to device. Length: {req.Length}");

        }

        // todo: add Dispose/d-tor

        bool CreateDigitalSession(string deviceUrl)
        {
            try
            {
                _deviceSession = new Session();
                _deviceSession.CreateDOChannel(deviceUrl);
                //_numberOfChannels = _deviceSession.GetNumberOfChannels();
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

        UInt16[] _lastScan;
        //class StatusStruct // might be generic
        //{
        //    UInt16[] _lastScan = new UInt16[Config.Instance.MaxDigital403OutputChannels];
        //    public UInt16[] LastScan { get => _lastScan; set => _lastScan = value; }
        //}
        //StatusStruct _status = new StatusStruct();
        public override string GetFormattedStatus()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder("Output bits: ");
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
        void f()
        {
            int i = 0;
            string s = Convert.ToString(i, 2);
        }

    }
}

