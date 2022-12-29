using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UeiDaq;
using UeiBridgeTypes;

namespace UeiBridge
{
    /// <summary>
    /// ** from the manual:
    /// ** Sequential Sampling, 16-bit, 24-channel Analog Input board
    /// </summary>
    class AI201InputDeviceManager : InputDevice
    {
        AnalogScaledReader _reader;
        log4net.ILog _logger = StaticMethods.GetLogger();
        public override string DeviceName => "AI-201-100"; // 
        IConvert _attachedConverter;
        public override IConvert AttachedConverter => _attachedConverter;

        public override string InstanceName => _instanceName;
        string _instanceName;

        //public AI201InputDeviceManager(IEnqueue<ScanResult> targetConsumer, TimeSpan samplingInterval, string caseUrl) : base(targetConsumer, samplingInterval, caseUrl)
        //{
        //    _channelsString = "Ai0,1,2,3";
        //    _attachedConverter = StaticMethods.CreateConverterInstance(DeviceName);
        //}

        public AI201InputDeviceManager(ISend<SendObject> targetConsumer, DeviceSetup setup): base( targetConsumer, setup)
        {
            _channelsString = "Ai0,1,2,3";
            _attachedConverter = StaticMethods.CreateConverterInstance(DeviceName);
            _instanceName = $"{DeviceName}/{setup.SlotNumber}";
        }

        public AI201InputDeviceManager(): base(null, null) // must have default const.
        {
        }

        bool OpenDevice(string deviceUrl)
        {
            _logger.Debug("not ready yet     tbd");
            return false;

            try
            {
                _deviceSession = new Session();
                _deviceSession.CreateAIChannel(deviceUrl, -15.0, 15.0, AIChannelInputMode.SingleEnded); // -15,15 means 'no gain'
                //_numberOfChannels = _deviceSession.GetNumberOfChannels();
                _deviceSession.ConfigureTimingForSimpleIO();
                _reader = new AnalogScaledReader(_deviceSession.GetDataStream());

//                _logger.Info($"{DeviceName}(Output) init success. { _deviceSession.GetNumberOfChannels()} channels. Range {range[0].minimum},{range[0].maximum}.");

//                _isDeviceReady = true;

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
            try
            {
                // read from device
                // ===============
                _lastScan = _reader.ReadSingleScan(); // access violation?
                System.Diagnostics.Debug.Assert(_lastScan != null);
                
                System.Diagnostics.Debug.Assert(_lastScan.Length == _deviceSession.GetNumberOfChannels(), "wrong number of channels");

                ScanResult dr = new ScanResult(_lastScan, this);
                //_targetConsumer.Enqueue(dr); tbd
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
            }
        }

        public override void OpenDevice()
        //public override void Start()
        {
            if ((_deviceSession!=null) && _deviceSession.IsRunning())
            {
                _logger.Warn("Can't start since device already running");
                return;
            }
            try
            {
                string deviceIndex = StaticMethods.FindDeviceIndex( Config2.Instance.CubeUrlList[0], DeviceName);
                if (null == deviceIndex)
                {
                    _logger.Warn($"Can't find index for device {DeviceName}");
                    return;
                }
                string url1 = _cubeUrl + deviceIndex + _channelsString;

                if (OpenDevice(url1))
                {
                    
                    var r = _deviceSession.GetDevice().GetAIRanges();
                    _logger.Info($"Init success { DeviceName}(input) . {_deviceSession.GetNumberOfChannels()}ch. Range {r[0].minimum},{r[0].maximum}. {deviceIndex + _channelsString}");
                }
                else
                {
                    _logger.Warn($"Device {DeviceName} init fail");
                    return;
                }
                _samplingTimer = new System.Threading.Timer(HandleResponse_Callback, null, TimeSpan.Zero, _samplingInterval);

            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
            }        
        }
        public override void Dispose()
        {
            _samplingTimer.Dispose();
            System.Threading.Thread.Sleep(200);
            CloseDevice();
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

