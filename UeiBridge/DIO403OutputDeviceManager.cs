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
    /// ** 48-channel Digital I/O **
    /// </summary>
    public class DIO403OutputDeviceManager : OutputDevice
    {
        public override string DeviceName => "DIO-403";

        private log4net.ILog _logger = StaticMethods.GetLogger();
        private IConvert2<UInt16[]> _attachedConverter;
        private IWriterAdapter<UInt16[]> _digitalWriter;
        //private Session _session;
        private System.Collections.Generic.List<ViewerItem<UInt16>> _viewerItemist;

        public DIO403OutputDeviceManager(DeviceSetup setup, IWriterAdapter<UInt16[]> digitalWriter, UeiDaq.Session session) : base(setup)
        {
            this._digitalWriter = digitalWriter;
            this._deviceSession = session;
        }
        public DIO403OutputDeviceManager() : base(null) { }// must have default c-tor

        public override void Dispose()
        {
            _digitalWriter.Dispose();
            base.CloseCurrentSession();
            base.Dispose();
            
        }

        public override bool OpenDevice()
        {
            _attachedConverter = new DigitalConverter(); //DIO403Convert(_digitalWriter.OriginSession.GetNumberOfChannels());

            int noOfbits = _deviceSession.GetNumberOfChannels() * 8;
            int firstBit = _deviceSession.GetChannel(0).GetIndex() * 8;
            _logger.Info($"Init success: {InstanceName}. Bits {firstBit}..{firstBit + noOfbits - 1} as output. Listening on {_deviceSetup.LocalEndPoint.ToIpEp()}"); // { noOfCh} output channels

            _viewerItemist = new System.Collections.Generic.List<ViewerItem<UInt16>>(new ViewerItem<UInt16>[_deviceSession.GetNumberOfChannels()]);

            Task.Factory.StartNew(() => OutputDeviceHandler_Task());
            _isDeviceReady = true;
            return false;
        }

        public override string [] GetFormattedStatus( TimeSpan interval)
        {
            System.Text.StringBuilder formattedString = new System.Text.StringBuilder("Output bits: ");
            lock (_viewerItemist)
            {
                if (_viewerItemist[0]?.timeToLive.Ticks > 0)
                {
                    _viewerItemist[0].timeToLive -= interval;
                    foreach (var vi in _viewerItemist)
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
            lock (_viewerItemist)
            {
                for (int ch = 0; ch < scan.Length; ch++)
                {
                    _viewerItemist[ch] = new ViewerItem<UInt16>(scan[ch], timeToLiveMs: 5000);
                }
            }
        }
    }
}

