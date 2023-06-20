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

        //private DigitalReader _reader;
        private log4net.ILog _logger = StaticMethods.GetLogger();
        private IConvert2<UInt16[]> _attachedConverter;
        private DIO403Setup _thisDeviceSetup;
        //private UInt16[] _lastScan;
        private System.Threading.Timer _samplingTimer;
        //private const string _channelsString = "Di1,3,5"; // 3 * 8 last bits as 'out'
        private Session _ueiSession;
        private ISend<SendObject> _targetConsumer;
        private byte[] _fixedScan8b;
        private List<byte> _scanMask = new List<byte>();
        private IReaderAdapter<UInt16[]> _digitalReader;
        private const int _numberOfLines = 48; // 48 bits

        public DIO403InputDeviceManager(DeviceSetup setup, IReaderAdapter<UInt16[]> digitalReader, Session ueiSession, ISend<SendObject> targetConsumer): base (setup)
        {
            _thisDeviceSetup = setup as DIO403Setup;
            _digitalReader = digitalReader;
            _ueiSession = ueiSession;
            _targetConsumer = targetConsumer;

            _attachedConverter = new DigitalConverter();// StaticMethods.CreateConverterInstance( setup);

            System.Diagnostics.Debug.Assert(null != setup);
            System.Diagnostics.Debug.Assert(DeviceName.Equals(setup.DeviceName));

        }
        public DIO403InputDeviceManager()  { }// must have default c-tor.
        public override bool OpenDevice()
        {
            // build scan-mask
            for(int i=0; i < _numberOfLines/8; i++)
            {
                _scanMask.Add(0);
            }
            foreach (Channel ch in _ueiSession.GetChannels())
            {
                _scanMask[ch.GetIndex()] = 0xff;
            }

            try
            {
                // emit log message
                string res = _ueiSession.GetChannel(0).GetResourceName();
                string localpath = (new Uri(res)).LocalPath;
                EmitInitMessage($"Init success: {DeviceName}. As {localpath}. Dest: {_thisDeviceSetup.DestEndPoint.ToIpEp()}"); // { noOfCh} output channels

                // make sampling timer
                TimeSpan interval = TimeSpan.FromMilliseconds(_thisDeviceSetup.SamplingInterval);
                _samplingTimer = new System.Threading.Timer(DeviceScan_Callback, null, TimeSpan.Zero, interval);

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                _ueiSession = null;
                return false;
            }
        }
        public override void Dispose()
        {
            _logger.Debug($"Disposing {this.DeviceName}/Input, slot {_thisDeviceSetup.SlotNumber}");
            _samplingTimer?.Dispose();
            System.Threading.Thread.Sleep(200); // wait for callback to finish
            CloseSession(_ueiSession);
        }

        public void DeviceScan_Callback(object state)
        {
            ushort[] singleScanFixed = new ushort[6];
            Array.Clear(singleScanFixed, 0, singleScanFixed.Length);

            // read from device
            // ===============
            try
            {
                UInt16[] singleScan = _digitalReader.ReadSingleScan();
            
                // fix to full buffer
                int i = 0;
                foreach( Channel ch in _ueiSession.GetChannels())
                {
                    singleScanFixed[ch.GetIndex()] = singleScan[i++];
                }
                // make EthernetMessage
                _fixedScan8b = _attachedConverter.UpstreamConvert(singleScanFixed);
                var ethMsg = StaticMethods.BuildEthernetMessageFromDevice( _fixedScan8b, _thisDeviceSetup);
                // send
                _targetConsumer.Send(new SendObject(_thisDeviceSetup.DestEndPoint.ToIpEp(), ethMsg.GetByteArray( MessageWay.upstream)));
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
            }
        }

        
        public override string [] GetFormattedStatus( TimeSpan interval)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder("Input bits: ");

            for(int i=0; i< _fixedScan8b.Length; i++)
            {
                if (_scanMask[i]>0)
                {
                    sb.Append(Convert.ToString(_fixedScan8b[i], 2).PadLeft(8, '0'));
                    sb.Append(" ");
                }
                else
                {
                    sb.Append("XXXXXXXX ");
                }
            }
            return new string[]{ sb.ToString() };
        }
    }
}
