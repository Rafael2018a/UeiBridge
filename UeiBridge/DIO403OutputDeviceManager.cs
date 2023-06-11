using System;
using System.Threading.Tasks;
//using UeiDaq;
using UeiBridge.Types;
using System.Timers;
using UeiBridge.Library;
using UeiDaq;
using System.Collections.Generic;
using System.Text;

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
        private DigitalConverter _digitalConverter = new DigitalConverter();
        private IWriterAdapter<UInt16[]> _digitalWriter;
        private System.Collections.Generic.List<ViewItem<UInt16>> _viewerItemist;
        private Session _ueiSession;
        private DeviceSetup _deviceSetup;

        public DIO403OutputDeviceManager(DeviceSetup setup, IWriterAdapter<UInt16[]> digitalWriter, UeiDaq.Session session) : base(setup)
        {
            this._digitalWriter = digitalWriter;
            this._ueiSession = session;
            this._deviceSetup = setup;
        }
        public DIO403OutputDeviceManager() { }// must have default c-tor

        public override void Dispose()
        {
            _digitalWriter.Dispose();
            //CloseSession(_ueiSession);
            try
            {
                _ueiSession.Stop();
            }
            catch (UeiDaq.UeiDaqException ex)
            {
                _logger.Debug($"Session stop() failed. {ex.Message}");
            }
            _ueiSession.Dispose();

            base.Dispose();

        }

        public override bool OpenDevice()
        {
            //List<int> channelIndexList = new List<int>();
            //StringBuilder sb;
            //foreach (Channel ch in _ueiSession.GetChannels())
            //{
            //    channelIndexList.Add( ch.GetIndex());
            //}

            string res = _ueiSession.GetChannel(0).GetResourceName();
            string localpath = (new Uri(res)).LocalPath;
            //int i = res.LastIndexOf("/");
            //string res1 = res.Substring(++i);
            EmitInitMessage($"Init success: {DeviceName}. As {localpath}. Listening on {_deviceSetup.LocalEndPoint.ToIpEp()}"); // { noOfCh} output channels

            _viewerItemist = new List<ViewItem<ushort>>(new ViewItem<UInt16>[_ueiSession.GetNumberOfChannels()]);

            Task.Factory.StartNew(() => OutputDeviceHandler_Task());
            _isDeviceReady = true;
            return false;
        }

        public override string[] GetFormattedStatus(TimeSpan interval)
        {
            System.Text.StringBuilder formattedString = new System.Text.StringBuilder("Output bits: ");
            lock (_viewerItemist)
            {
                if (_viewerItemist[0]?.timeToLive.Ticks > 0)
                {
                    _viewerItemist[0].timeToLive -= interval;
                    foreach (var vi in _viewerItemist)
                    {
                        if (null == vi)
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
            var ls = _digitalConverter.DownstreamConvert(request.PayloadBytes);
            ushort[] scan = ls as ushort[];
            System.Diagnostics.Debug.Assert(scan != null);
            _digitalWriter.WriteSingleScan(scan);
            lock (_viewerItemist)
            {
                for (int ch = 0; ch < scan.Length; ch++)
                {
                    _viewerItemist[ch] = new ViewItem<UInt16>(scan[ch], timeToLiveMs: 5000);
                }
            }
        }
    }
}

