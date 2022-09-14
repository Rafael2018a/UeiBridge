﻿using System;
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
        log4net.ILog _logger = log4net.LogManager.GetLogger("Root");
        public override string DeviceName => "AI-201-100";
        IConvert _attachedConverter;
        public override IConvert AttachedConverter => _attachedConverter;

        public AI201InputDeviceManager(IEnqueue<ScanResult> targetConsumer, TimeSpan samplingInterval, string caseUrl) : base(targetConsumer, samplingInterval, caseUrl)
        {
            _channelsString = "Ai0,1,2,3";
            _attachedConverter = StaticMethods.CreateConverterInstance(DeviceName);
        }


        bool OpenDevice(string deviceUrl)
        {
            try
            {
                _deviceSession = new Session();
                _deviceSession.CreateAIChannel(deviceUrl, -15.0, 15.0, AIChannelInputMode.SingleEnded); // -15,15 means 'no gain'
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
            if ((null == _deviceSession)||(null==_reader))
            {
                lock (this)
                {
                    if ((null == _deviceSession) || (null == _reader))
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
                            var r = _deviceSession.GetDevice().GetAIRanges();
                            _logger.Info($"{DeviceName}(input) init success. {_numberOfChannels} channels. Range {r[0].minimum},{r[0].maximum}. {deviceIndex + _channelsString}");
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
            _lastScan = _reader.ReadSingleScan();
            System.Diagnostics.Debug.Assert(_lastScan != null);
            System.Diagnostics.Debug.Assert(_lastScan.Length == _numberOfChannels, "wrong number of channels");

            ScanResult dr = new ScanResult(_lastScan, this);
            _targetConsumer.Enqueue(dr);
        }

        public override void Start()
        {
            _samplingTimer = new System.Threading.Timer(HandleResponse_Callback, null, TimeSpan.Zero, _samplingInterval);
        }

        double[] _lastScan;

        

        public override string GetFormattedStatus()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder("Input voltage: ");
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
    }
}

