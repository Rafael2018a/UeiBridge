using System;
using UeiDaq;
using UeiBridge.Types;
using UeiBridge.Library;
using System.Collections.Generic;


namespace UeiBridge
{
    /// <summary>
    /// from the manual:
    /// ** 48-channel Digital I/O **
    /// </summary>
    public class DIO403InputDeviceManager : InputDevice
    {
        public override string DeviceName => "DIO-403";
        public ISend<SendObject> TargetConsumer { get => _targetConsumer; set => _targetConsumer = value; }

        private DigitalReader _reader;
        private log4net.ILog _logger = StaticMethods.GetLogger();
        private IConvert2<UInt16[]> _attachedConverter;
        private DIO403Setup _thisDeviceSetup;
        private UInt16[] _lastScan;
        private System.Threading.Timer _samplingTimer;
        private const string _channelsString = "Di3:5"; // 3 * 8 last bits as 'out'
        private Session _ueiSession;
        private ISend<SendObject> _targetConsumer;
        public DIO403InputDeviceManager( ISend<SendObject> targetConsumer, DeviceSetup setup): base( setup)
        {
            
            _attachedConverter = new DigitalConverter();// StaticMethods.CreateConverterInstance( setup);
            _thisDeviceSetup = setup as DIO403Setup;
            _targetConsumer = targetConsumer;

            System.Diagnostics.Debug.Assert(null != setup);
            System.Diagnostics.Debug.Assert(DeviceName.Equals(setup.DeviceName));
        }
        public DIO403InputDeviceManager()  { }// must have default c-tor.
        public override void OpenDevice()
        {
            string deviceUrl = $"{_thisDeviceSetup.CubeUrl}Dev{_thisDeviceSetup.SlotNumber}/{_channelsString}";

            try
            {
                _ueiSession = new Session();
                _ueiSession.CreateDIChannel(deviceUrl);
                _ueiSession.ConfigureTimingForSimpleIO();
                _reader = new DigitalReader(_ueiSession.GetDataStream());

                int noOfbits = _ueiSession.GetNumberOfChannels() * 8;
                int firstBit = _ueiSession.GetChannel(0).GetIndex() * 8;
                EmitInitMessage( $"Init success: {DeviceName}. Bits {firstBit}..{firstBit + noOfbits - 1} as input. Dest: {_thisDeviceSetup.DestEndPoint.ToIpEp()}");
                TimeSpan interval = TimeSpan.FromMilliseconds(_thisDeviceSetup.SamplingInterval);
                _samplingTimer = new System.Threading.Timer(DeviceScan_Callback, null, TimeSpan.Zero, interval);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                _ueiSession = null;
            }
        }
        public override void Dispose()
        {
            _logger.Debug($"Disposing {this.DeviceName}/Input, slot {_thisDeviceSetup.SlotNumber}");
            _samplingTimer?.Dispose();
            System.Threading.Thread.Sleep(200); // wait for callback to finish
            CloseSession(_ueiSession);
			base.Dispose();
        }
        public void DeviceScan_Callback(object state)
        {
            // read from device
            // ===============
            try
            {
                _lastScan = _reader.ReadSingleScanUInt16();
                System.Diagnostics.Debug.Assert(_lastScan != null);
                System.Diagnostics.Debug.Assert(_lastScan.Length == _ueiSession.GetNumberOfChannels(), "wrong number of channels");
                byte[] payload = _attachedConverter.UpstreamConvert(_lastScan);
                var em = StaticMethods.BuildEthernetMessageFromDevice( payload, _thisDeviceSetup);

                _targetConsumer.Send(new SendObject(_thisDeviceSetup.DestEndPoint.ToIpEp(), em.GetByteArray( MessageWay.upstream)));
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
            }
        }

        
        public override string [] GetFormattedStatus( TimeSpan interval)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder("Input bits: ");
            if (null != _lastScan)
            {
                foreach (UInt16 val in _lastScan)
                {
                    sb.Append(Convert.ToString(val, 2).PadLeft(8, '0'));
                    sb.Append("  ");
                }
            }
            return new string[]{ sb.ToString() };
        }
    }
}
