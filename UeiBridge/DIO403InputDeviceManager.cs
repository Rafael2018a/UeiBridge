using System;
//using UeiDaq;
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

        private log4net.ILog _logger = StaticMethods.GetLogger();
        private DigitalConverter _digitalConverter = new DigitalConverter();
        private DIO403Setup _thisDeviceSetup;
        private System.Threading.Timer _samplingTimer;
        private byte[] _fullBuffer8bit;
        private List<byte> _scanMask = new List<byte>();
        private IReaderAdapter<UInt16[]> _ueiDigitalReader;

        public DIO403InputDeviceManager(DIO403Setup setup, ISession ueiSession, ISend<SendObject> targetConsumer): base (setup)
        {
            _thisDeviceSetup = setup;
            _ueiSession = ueiSession;
            TargetConsumer = targetConsumer;
            _ueiDigitalReader = _ueiSession.GetDigitalReader();

            //System.Diagnostics.Debug.Assert(null != setup);
            //System.Diagnostics.Debug.Assert(DeviceName.Equals(setup.DeviceName));

        }
        public DIO403InputDeviceManager()  { }// must have default c-tor.
        public override bool OpenDevice()
        {
            // build scan-mask
            int numOfCh = _thisDeviceSetup.IOChannelList.Count;
            byte[] ba = new byte[numOfCh];
            Array.Clear(ba, 0, ba.Length);
            _scanMask = new List<byte>(ba);
            foreach (IChannel ch in _ueiSession.GetChannels())
            {
                int i = ch.GetIndex();
                _scanMask[i] = 0xff;
            }

            try
            {
                // emit log message
                //string res = UeiSession.GetChannel(0).GetResourceName();
                //string localpath = (new Uri(res)).LocalPath;
                EmitInitMessage($"Init success: {DeviceName}. Dest: {_thisDeviceSetup.DestEndPoint.ToIpEp()}"); // { noOfCh} output channels

                // make sampling timer
                TimeSpan interval = TimeSpan.FromMilliseconds(_thisDeviceSetup.SamplingInterval);
                _samplingTimer = new System.Threading.Timer(DeviceScan_Callback, null, TimeSpan.Zero, interval);

                _isDeviceReady = true;
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
            _ueiDigitalReader?.Dispose();

            base.Dispose();
        }

        public void DeviceScan_Callback(object state)
        {
            ushort[] fullBuffer16bit = new ushort[_thisDeviceSetup.IOChannelList.Count];
            Array.Clear(fullBuffer16bit, 0, fullBuffer16bit.Length);

            // read from device
            // ===============
            try
            {
                UInt16[] singleScan = _ueiDigitalReader.ReadSingleScan();
            
                // fix to full buffer
                int i = 0;
                foreach( IChannel ch in _ueiSession.GetChannels())
                {
                    fullBuffer16bit[ch.GetIndex()] = singleScan[i++];
                }
                // make EthernetMessage
                _fullBuffer8bit = _digitalConverter.UpstreamConvert(fullBuffer16bit);
                System.Diagnostics.Debug.Assert(null != _fullBuffer8bit);
                var ethMsg = StaticMethods.BuildEthernetMessageFromDevice( _fullBuffer8bit, _thisDeviceSetup);
                // send
                TargetConsumer.Send(new SendObject(_thisDeviceSetup.DestEndPoint.ToIpEp(), ethMsg.GetByteArray( MessageWay.upstream)));
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
            }
        }
        
        public override string [] GetFormattedStatus( TimeSpan interval)
        {
            if ((false == _isDeviceReady)||(null == _fullBuffer8bit))
            {
                return null;
            }
            System.Text.StringBuilder sb = new System.Text.StringBuilder("Input bits: ");

            for(int i=0; i< _fullBuffer8bit.Length; i++)
            {
                if (_scanMask[i]>0)
                {
                    sb.Append(Convert.ToString(_fullBuffer8bit[i], 2).PadLeft(8, '0'));
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
