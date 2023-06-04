using System;
using System.Threading.Tasks;
//using UeiDaq;
using UeiBridge.Types;
using System.Timers;
using UeiBridge.Library;
using UeiDaq;

namespace UeiBridge
{
    /// <summary>
    /// from the manual:
    /// ** 10 channel relay board **
    /// </summary>
    class DIO470OutputDeviceManager : OutputDevice
    {
        public override string DeviceName => "DIO-470";

        //privates
        log4net.ILog _logger = StaticMethods.GetLogger();
        //IConvert _attachedConverter;
        private IConvert2<UInt16[]> _digitalConverter = new DigitalConverter();
        const string _channelsString = "Do0";
        //UeiDaq.Session _deviceSession;
        UeiDaq.DigitalWriter _writer;
        UInt16[] _lastScan;
        private Session _ueiSession;
        private DeviceSetup _deviceSetup;
        public DIO470OutputDeviceManager(DeviceSetup setup) : base(setup)
        {
            this._deviceSetup = setup;
        }
        public DIO470OutputDeviceManager()  // must have default c-tor
        {
        }

        public override void Dispose()
        {
            _writer.Dispose();
            CloseSession(_ueiSession);
			base.Dispose();
        }

        public override bool OpenDevice()
        {
            //_attachedConverter = StaticMethods.CreateConverterInstance( _deviceSetup);
            string cubeUrl = $"{_deviceSetup.CubeUrl}Dev{_deviceSetup.SlotNumber}/{_channelsString}";
            _ueiSession = new UeiDaq.Session();
            _ueiSession.CreateDOChannel(cubeUrl);

            _ueiSession.ConfigureTimingForSimpleIO();
            _writer = new UeiDaq.DigitalWriter(_ueiSession.GetDataStream());

            //UInt16[] u16 = { 0x1 };
            //_writer.WriteSingleScanUInt16(u16);

            int noOfbits = 10;// _deviceSession.GetNumberOfChannels() * 8;
            int firstBit = 0;// _deviceSession.GetChannel(0).GetIndex() * 8;
            EmitInitMessage( $"Init success: {DeviceName}. Bits {firstBit}..{firstBit + noOfbits - 1} as output. Dest{_deviceSetup.LocalEndPoint?.ToIpEp().ToString()}"); 
            //_logger.Info($"Init success: {InstanceName}. Bits {firstBit}..{firstBit + noOfbits - 1} as output"); // { noOfCh} output channels

            Task.Factory.StartNew(() => OutputDeviceHandler_Task());
            _isDeviceReady = true;
            return false;
        }

        public override string [] GetFormattedStatus( TimeSpan interval)
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
            return new string[] { sb.ToString() };
        }

        protected override void HandleRequest(EthernetMessage request)
        {
            var ls = _digitalConverter.DownstreamConvert(request.PayloadBytes);
                //_attachedConverter.EthToDevice(request.PayloadBytes);
            if (null==ls)
            {
                _logger.Warn("Empty payload. rejected.");
                return;
            }
            ushort[] scan = ls as ushort[];
            System.Diagnostics.Debug.Assert( scan != null);
            _writer.WriteSingleScanUInt16( scan);
            _lastScan = scan;
        }
    }
}

