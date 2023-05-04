using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UeiDaq;
using UeiBridge.Types;
using UeiBridge.Library;

namespace UeiBridge
{
    /// <summary>
    /// ** from the manual:
    /// ** Sequential Sampling, 16-bit, 24-channel Analog Input board
    /// </summary>
    class AI201InputDeviceManager : InputDevice
    {
        public override string DeviceName => "AI-201-100"; // 

        private AnalogScaledReader _reader;
        private log4net.ILog _logger = StaticMethods.GetLogger();
        private IConvert2<double[]> _attachedConverter;
        private AI201100Setup _thisDeviceSetup;
        private double[] _lastScan;
        private System.Threading.Timer _samplingTimer;

        public AI201InputDeviceManager(ISend<SendObject> targetConsumer, AI201100Setup setup) : base(targetConsumer, setup)
        {
            _channelsString = "Ai0:23";
            _attachedConverter = new AnalogConverter(AI201100Setup.PeekVoltage_upstream, AO308Setup.PeekVoltage_downstream);
            
            _targetConsumer = targetConsumer;
            _thisDeviceSetup = setup;
            System.Diagnostics.Debug.Assert(this.DeviceName.Equals(setup.DeviceName));
        }

        public AI201InputDeviceManager() : base(null, null) { }// must have default const.

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
                byte[] payload = _attachedConverter.UpstreamConvert(_lastScan);
                EthernetMessage em = StaticMethods.BuildEthernetMessageFromDevice(payload, this._thisDeviceSetup);
                SendObject so = new SendObject(_thisDeviceSetup.DestEndPoint.ToIpEp(), em.GetByteArray( MessageWay.upstream));
                _targetConsumer.Send(so);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
            }
        }

        public override void OpenDevice()
        {
            double peek = AI201100Setup.PeekVoltage_upstream;
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
            _reader.Dispose();
            CloseCurrentSession();
            base.Dispose();
        }

        public override string []GetFormattedStatus( TimeSpan interval)
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
            return new string[] { sb.ToString() };
        }
    }
}

