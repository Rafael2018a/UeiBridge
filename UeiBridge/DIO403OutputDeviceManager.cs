using System;
using System.Threading.Tasks;
using UeiDaq;

namespace UeiBridge
{
    /// <summary>
    /// from the manual:
    /// ** 48-channel Digital I/O **
    /// </summary>
    class DIO403OutputDeviceManager : OutputDevice //DioOutputDeviceManager
    {
        log4net.ILog _logger = StaticMethods.GetLogger();
        //protected override IConvert AttachedConverter => _attachedConverter; // tbd. remove this
        private IConvert _attachedConverter;
        string _channelsString;
        string _instanceName;
        Session _deviceSession;
        UeiDaq.DigitalWriter _writer;
        UInt16[] _lastScan;
        public DIO403OutputDeviceManager( DeviceSetup setup): base( setup)
        {
            _channelsString = "Do0:2"; // first 24 bits as 'out'
            _instanceName = $"{DeviceName}/Slot{ setup.SlotNumber}";
        }

        public override string DeviceName => "DIO-403";

        //protected override string ChannelsString => _channelsString;

        public override string InstanceName => _instanceName;

        public override void Dispose()
        {
            //OutputDevice deviceManager = ProjectRegistry.Instance.OutputDevicesMap[DeviceName];
            //DeviceRequest dr = new DeviceRequest(OutputDevice.CancelTaskRequest, "");
            //deviceManager.Enqueue(dr);
            //System.Threading.Thread.Sleep(100);
            base.Dispose();
        }

        public override bool OpenDevice()
        {
            _attachedConverter = StaticMethods.CreateConverterInstance(DeviceName);
            string cubeUrl = $"{_deviceSetup.CubeUrl}Dev{_deviceSetup.SlotNumber}/{_channelsString}";
            _deviceSession = new UeiDaq.Session();
            _deviceSession.CreateDOChannel(cubeUrl);
            
            _deviceSession.ConfigureTimingForSimpleIO();
            _writer = new UeiDaq.DigitalWriter(_deviceSession.GetDataStream());

            Task.Factory.StartNew(() => OutputDeviceHandler_Task());

            int noOfbits = _deviceSession.GetNumberOfChannels() * 8;
            int firstBit = _deviceSession.GetChannel(0).GetIndex()*8;
            _logger.Info($"{DeviceName} init success. . Bits {firstBit}..{firstBit+noOfbits - 1} as output" ); // { noOfCh} output channels

            _isDeviceReady = true;
            return false;

        }


        public override string GetFormattedStatus()
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
            return sb.ToString();
        }

        protected override void HandleRequest(EthernetMessage request)
        {
            _logger.Debug("HandleRequest... tbd");
        }
    }
}

