﻿using System;
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

        public override string DeviceName => "AO-308";
        string _channelsString;
        public override IConvert AttachedConverter => _attachedConverter;

        protected override string ChannelsString => throw new NotImplementedException();

        readonly IConvert _attachedConverter;
        public AO308OutputDeviceManager( DeviceSetup deviceSetup): base( deviceSetup)
        {
            _channelsString = "Ao0:7";
            _attachedConverter = StaticMethods.CreateConverterInstance(DeviceName);
        }


        public override bool  OpenDevice()
        {

            try
            {
                string cubeUrl = $"{_deviceSetup.CubeUrl}Dev{_deviceSetup.SlotNumber}/{_channelsString}";

                _deviceSession = new Session();
				//var minmax = Config.Instance.Analog_Out_MinMaxVoltage;
                var c = _deviceSession.CreateAOChannel(cubeUrl, -Config.Instance.Analog_Out_PeekVoltage, Config.Instance.Analog_Out_PeekVoltage);
                System.Diagnostics.Debug.Assert(c.GetMaximum() == Config.Instance.Analog_Out_PeekVoltage);
                //_numberOfChannels = _deviceSession.GetNumberOfChannels();
                _deviceSession.ConfigureTimingForSimpleIO();
                _writer = new AnalogScaledWriter(_deviceSession.GetDataStream());

                var range = _deviceSession.GetDevice().GetAORanges();

                Task.Factory.StartNew(() => OutputDeviceHandler_Task());

                _logger.Info($"{DeviceName}(Output) init success. { _deviceSession.GetNumberOfChannels()} channels. Range {range[0].minimum},{range[0].maximum}.");

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

        protected override void HandleRequest(DeviceRequest request)
        {
            throw new NotImplementedException();
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
            //t1.Dispose();
            //t1 = null;
            OutputDevice deviceManager = ProjectRegistry.Instance.OutputDevicesMap[DeviceName];
            DeviceRequest dr = new DeviceRequest( OutputDevice.CancelTaskRequest, "");
            deviceManager.Enqueue(dr);
            System.Threading.Thread.Sleep(100);
            CloseSession();
        }

    }
}