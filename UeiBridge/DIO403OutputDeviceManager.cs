using System;
using System.Threading.Tasks;
//using UeiDaq;
using UeiBridgeTypes;
using System.Timers;

namespace UeiBridge
{
    /// <summary>
    /// from the manual:
    /// ** 48-channel Digital I/O **
    /// </summary>
    class DIO403OutputDeviceManager : OutputDevice //DioOutputDeviceManager
    {
        public override string DeviceName => "DIO-403";
        public override string InstanceName { get; }

        //privates
        log4net.ILog _logger = StaticMethods.GetLogger();
        IConvert _attachedConverter;
        const string _channelsString = "Do0:2";// Do0:2 - 3*8 first bits as 'out'
        UeiDaq.Session _deviceSession;
        UeiDaq.DigitalWriter _writer;
        System.Collections.Generic.List<ViewerItem<UInt16>> _lastScanList;

        public DIO403OutputDeviceManager(DeviceSetup setup) : base(setup)
        {
            InstanceName = $"{DeviceName}/Slot{ setup.SlotNumber}/Output";
        }
        public DIO403OutputDeviceManager() : base(null) // must have default c-tor
        {
        }

        public override void Dispose()
        {
            base.Dispose();

            if (null != _writer)
            {
                _writer.Dispose();
            }
            if (null != _deviceSession)
            {
                _deviceSession.Stop();
                _deviceSession.Dispose();
            }
        }

        public override bool OpenDevice()
        {
            _attachedConverter = StaticMethods.CreateConverterInstance( _deviceSetup);
            string cubeUrl = $"{_deviceSetup.CubeUrl}Dev{_deviceSetup.SlotNumber}/{_channelsString}";
            _deviceSession = new UeiDaq.Session();
            _deviceSession.CreateDOChannel(cubeUrl);

            _deviceSession.ConfigureTimingForSimpleIO();
            _writer = new UeiDaq.DigitalWriter(_deviceSession.GetDataStream());

            int noOfbits = _deviceSession.GetNumberOfChannels() * 8;
            int firstBit = _deviceSession.GetChannel(0).GetIndex() * 8;
            _logger.Info($"Init success: {InstanceName}. Bits {firstBit}..{firstBit + noOfbits - 1} as output. Listening on {_deviceSetup.LocalEndPoint.ToIpEp()}"); // { noOfCh} output channels

            _lastScanList = new System.Collections.Generic.List<ViewerItem<UInt16>>(new ViewerItem<UInt16>[_deviceSession.GetNumberOfChannels()]);

            Task.Factory.StartNew(() => OutputDeviceHandler_Task());
            _isDeviceReady = true;
            return false;
        }

        public override string GetFormattedStatus( TimeSpan interval)
        {
            System.Text.StringBuilder formattedString = new System.Text.StringBuilder("Output bits: ");
            lock (_lastScanList)
            {
                if (_lastScanList[0]?.timeToLive.Ticks > 0)
                {
                    _lastScanList[0].timeToLive -= interval;
                    foreach (var vi in _lastScanList)
                    {
                        formattedString.Append(Convert.ToString(vi.readValue, 2).PadLeft(8, '0'));
                        formattedString.Append("  ");
                    }
                }
                else
                {
                    formattedString.Append("- - -");
                }
            }
            return formattedString.ToString();
        }

        protected override void HandleRequest(EthernetMessage request)
        {
            var ls = _attachedConverter.EthToDevice(request.PayloadBytes);
            ushort[] scan = ls as ushort[];
            System.Diagnostics.Debug.Assert( scan != null);
            _writer.WriteSingleScanUInt16( scan);
            lock (_lastScanList)
            {
                for (int ch = 0; ch < scan.Length; ch++)
                {
                    _lastScanList[ch] = new ViewerItem<UInt16>(scan[ch], timeToLiveMs: 5000);
                }
            }
        }
    }
}

