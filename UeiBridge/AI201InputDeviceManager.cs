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
        public override string InstanceName { get; }//=> _instanceName;
        //ISend<SendObject> _targetConsumer;
        AI201100Setup _thisDeviceSetup;
        double[] _lastScan;

        public AI201InputDeviceManager(ISend<SendObject> targetConsumer, AI201100Setup setup) : base(targetConsumer)
        {
            _channelsString = "Ai0:23";
            _attachedConverter = StaticMethods.CreateConverterInstance( setup);
            InstanceName = $"{DeviceName}/Slot{setup.SlotNumber}/Input";
            _targetConsumer = targetConsumer;
            _thisDeviceSetup = setup;
            System.Diagnostics.Debug.Assert(this.DeviceName.Equals(setup.DeviceName));
        }

        public AI201InputDeviceManager() : base(null) // must have default const.
        {
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

                //ScanResult dr = new ScanResult(_lastScan, this);
                byte[] payload = _attachedConverter.DeviceToEth(_lastScan);
                EthernetMessage em = EthernetMessage.CreateFromDevice(payload, this._thisDeviceSetup);
                SendObject so = new SendObject(_thisDeviceSetup.DestEndPoint.ToIpEp(), em.ToByteArrayUp());
                _targetConsumer.Send(so);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
            }
        }

        public override void OpenDevice()
        {
            double peek = _thisDeviceSetup.PeekVoltage_In;
            try
            {
                string url1 = $"{_thisDeviceSetup.CubeUrl}Dev{_thisDeviceSetup.SlotNumber}/{_channelsString}";
                _deviceSession = new Session();
                _deviceSession.CreateAIChannel(url1, -peek, peek, AIChannelInputMode.SingleEnded); // -15,15 means 'no gain'
                var numberOfChannels = _deviceSession.GetNumberOfChannels();
                _deviceSession.ConfigureTimingForSimpleIO();
                _reader = new AnalogScaledReader(_deviceSession.GetDataStream());
                var r = _deviceSession.GetDevice().GetAIRanges();
                TimeSpan interval = TimeSpan.FromMilliseconds(_thisDeviceSetup.SamplingInterval);
                _samplingTimer = new System.Threading.Timer(HandleResponse_Callback, null, TimeSpan.Zero, interval);
                _logger.Info($"Init success. {InstanceName}. {_deviceSession.GetNumberOfChannels()} input channels. Dest:{_thisDeviceSetup.DestEndPoint.ToIpEp()}");
                return;

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

        //

        public override string GetFormattedStatus( TimeSpan interval)
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

