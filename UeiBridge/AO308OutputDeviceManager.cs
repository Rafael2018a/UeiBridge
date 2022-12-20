using System;
using System.Threading.Tasks;
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
        log4net.ILog _logger = StaticMethods.GetLogger();
        private string _instanceName;
        public override string DeviceName => "AO-308";
        const string _channelsString = "Ao0:7";
        Session _deviceSession;
        //protected override IConvert AttachedConverter => _attachedConverter;

        //protected override string ChannelsString => throw new NotImplementedException();

        public override string InstanceName =>  _instanceName;

        private IConvert _attachedConverter;
        public AO308OutputDeviceManager( DeviceSetup deviceSetup): base( deviceSetup)
        {
            _instanceName = $"{DeviceName}/Slot{deviceSetup.SlotNumber}/Output";
        }

        public AO308OutputDeviceManager(): base(null)
        {
        }

        public override bool  OpenDevice()
        {
            try
            {
                _attachedConverter = StaticMethods.CreateConverterInstance(DeviceName);

                string cubeUrl = $"{_deviceSetup.CubeUrl}Dev{_deviceSetup.SlotNumber}/{_channelsString}";

                _deviceSession = new Session();
                var c = _deviceSession.CreateAOChannel(cubeUrl, -Config.Instance.Analog_Out_PeekVoltage, Config.Instance.Analog_Out_PeekVoltage);
                System.Diagnostics.Debug.Assert(c.GetMaximum() == Config.Instance.Analog_Out_PeekVoltage);
                _deviceSession.ConfigureTimingForSimpleIO();
                _writer = new AnalogScaledWriter(_deviceSession.GetDataStream());

                Task.Factory.StartNew(() => OutputDeviceHandler_Task());

                var range = _deviceSession.GetDevice().GetAORanges();
                _logger.Info($"Init success: {InstanceName} . { _deviceSession.GetNumberOfChannels()} channels. Range {range[0].minimum},{range[0].maximum}V.");

                _isDeviceReady = true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                _deviceSession = null;
                return false;
            }
            return true;
        }

        protected override void HandleRequest( EthernetMessage em)
        {
            //DeviceRequest dr;
            // init session, if needed.
            // =======================
            //if ((null == _deviceSession) || (_caseUrl != dr.CaseUrl))
            //{
            //    CloseDevice();

            //    string deviceIndex = StaticMethods.FindDeviceIndex(DeviceName);
            //    if (null == deviceIndex)
            //    {
            //        _logger.Warn($"Can't find index for device {DeviceName}");
            //        return;
            //    }

            //    string url1 = dr.CaseUrl + deviceIndex + _channelsString;

            //    if (OpenDevice(url1))
            //    {
            //        var range = _deviceSession.GetDevice().GetAORanges();

            //        //_logger.Info($"{_deviceName} init success. {_numberOfChannels} output channels. {url1}");
            //        _logger.Info($"{DeviceName}(Output) init success. { _deviceSession.GetNumberOfChannels()} channels. Range {range[0].minimum},{range[0].maximum}. {deviceIndex + _channelsString}");
            //        _caseUrl = dr.CaseUrl;
            //    }
            //    else
            //    {
            //        _logger.Warn($"Device {DeviceName} init fail");
            //        return;
            //    }
            //}

            // write to device
            // ===============
            var p = _attachedConverter.EthToDevice(em.PayloadBytes);
            if (_isDeviceReady)
            {
                _lastScan = p as double[];
                if (null != _lastScan)
                {
                    _writer.WriteSingleScan(_lastScan);
                    //_logger.Debug($"AO voltage {_lastScan[0]}");
                    //_logger.Debug($"scan written to device. Length: {_lastScan.Length}");
                }
            }
        }

        //StatusStruct _status = new StatusStruct();
        double[] _lastScan;

        public override string GetFormattedStatus()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder("Output voltage: ");
            if (null != _lastScan)
            {
                foreach (double d in _lastScan)
                {
                    sb.Append("  ");
                    sb.Append(d.ToString("0.0"));
                }
            }
            return sb.ToString();
        }

        public override void Dispose()
        {
            base.Dispose();

            if (null != _writer)
            {
                _writer.Dispose();
            }
            if (null != _deviceSession)
            {
                _deviceSession.Stop();
                _deviceSession.Dispose();
            }
        }
        public virtual void Dispose1()
        {
            _deviceSession = null;
        }


    }
}