using System;
using System.Threading.Tasks;
//using UeiDaq;
using UeiBridge.Types;
using System.Timers;
using UeiBridge.Library;
using UeiDaq;

namespace UeiBridge
{

    public class DigitalWriterAdapter : IWriterAdapter<UInt16[]>
    {
        UeiDaq.DigitalWriter _ueiDigitalWriter;
        Session _originSession;
        public Session OriginSession => _originSession;

        public DigitalWriterAdapter(DigitalWriter ueiDigitalWriter, Session originSession)
        {
            _ueiDigitalWriter = ueiDigitalWriter;
            _originSession = originSession;
        }

        public void WriteSingleScan(ushort[] scan)
        {
            _ueiDigitalWriter.WriteSingleScanUInt16(scan);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// from the manual:
    /// ** 48-channel Digital I/O **
    /// </summary>
    public class DIO403OutputDeviceManager : OutputDevice
    {
        public override string DeviceName => "DIO-403";

        //privates
        log4net.ILog _logger = StaticMethods.GetLogger();
        IConvert2<UInt16[]> _attachedConverter;
        const string _channelsString = "Do0:2";// Do0:2 - 3*8 first bits as 'out'
        IWriterAdapter<UInt16[]> _digitalWriter;
        private Session _session;

        public IWriterAdapter<UInt16[]> DigitalWriter => _digitalWriter;
        System.Collections.Generic.List<ViewerItem<UInt16>> _lastScanList;

        public DIO403OutputDeviceManager(DeviceSetup setup, IWriterAdapter<UInt16[]> digitalWriter, UeiDaq.Session session) : base(setup)
        {
            _digitalWriter = digitalWriter;
            this._session = session;
        }
        public DIO403OutputDeviceManager() : base(null) // must have default c-tor
        {
        }

        public override void Dispose()
        {
            base.Dispose();

            //if (null != _writer)
            //{
            //    _writer.Dispose();
            //}
            if (null != _session)
            {
                _session.Stop();
                _session.Dispose();
            }
        }

        public override bool OpenDevice()
        {
            _attachedConverter = new DigitalConverter(); //DIO403Convert(_digitalWriter.OriginSession.GetNumberOfChannels());

            int noOfbits = _session.GetNumberOfChannels() * 8;
            int firstBit = _session.GetChannel(0).GetIndex() * 8;
            _logger.Info($"Init success: {InstanceName}. Bits {firstBit}..{firstBit + noOfbits - 1} as output. Listening on {_deviceSetup.LocalEndPoint.ToIpEp()}"); // { noOfCh} output channels

            _lastScanList = new System.Collections.Generic.List<ViewerItem<UInt16>>(new ViewerItem<UInt16>[_session.GetNumberOfChannels()]);

            Task.Factory.StartNew(() => OutputDeviceHandler_Task());
            _isDeviceReady = true;
            return false;
        }

        public override string [] GetFormattedStatus( TimeSpan interval)
        {
            System.Text.StringBuilder formattedString = new System.Text.StringBuilder("Output bits: ");
            lock (_lastScanList)
            {
                if (_lastScanList[0]?.timeToLive.Ticks > 0)
                {
                    _lastScanList[0].timeToLive -= interval;
                    foreach (var vi in _lastScanList)
                    {
                        if (null==vi)
                        {
                            continue;
                        }
                        formattedString.Append(Convert.ToString(vi.readValue, 2).PadLeft(8, '0'));
                        formattedString.Append("  ");
                    }
                }
                else
                {
                    formattedString.Append("- - -");
                }
            }
            return new string[] { formattedString.ToString() };
        }

        protected override void HandleRequest(EthernetMessage request)
        {
            var ls = _attachedConverter.DownstreamConvert( request.PayloadBytes);
            ushort[] scan = ls as ushort[];
            System.Diagnostics.Debug.Assert( scan != null);
            _digitalWriter.WriteSingleScan( scan);
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

