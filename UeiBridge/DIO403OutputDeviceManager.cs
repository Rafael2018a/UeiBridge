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
        private List<ViewItem<ushort>> _viewerItemist;
        private ViewItem<byte[]> _viewItem;
        private Session _ueiSession;
        private DeviceSetup _deviceSetup;
        private List<byte> _scanMask = new List<byte>();
        private const int _numberOfLines = 48; // 48 bits

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
            // build scan-mask
            for (int i = 0; i < _numberOfLines / 8; i++)
            {
                _scanMask.Add(0);
            }
            foreach (Channel ch in _ueiSession.GetChannels())
            {
                _scanMask[ch.GetIndex()] = 0xff;
            }

            string res = _ueiSession.GetChannel(0).GetResourceName();
            string localpath = (new Uri(res)).LocalPath;
            EmitInitMessage($"Init success: {DeviceName}. As {localpath}. Listening on {_deviceSetup.LocalEndPoint.ToIpEp()}"); // { noOfCh} output channels

            //_viewerItemist = new List<ViewItem<ushort>>(new ViewItem<UInt16>[_ueiSession.GetNumberOfChannels()]);

            Task.Factory.StartNew(() => OutputDeviceHandler_Task());
            _isDeviceReady = true;
            return false;
        }

        public override string[] GetFormattedStatus(TimeSpan interval)
        {
            StringBuilder sb = new System.Text.StringBuilder("Output bits: ");
            if (null == _viewItem?.readValue)
            {
                return null;
            }
            if (_viewItem.timeToLive.Ticks > 0)
            {
                _viewItem.timeToLive -= interval;
                for (int i = 0; i < _viewItem.readValue.Length; i++)
                {
                    if (_scanMask[i] > 0)
                    {
                        sb.Append(Convert.ToString(_viewItem.readValue[i], 2).PadLeft(8, '0'));
                        sb.Append(" ");
                    }
                    else
                    {
                        sb.Append("XXXXXXXX ");
                    }
                }
            }
            return new string[] { sb.ToString() };
        }

        public  string[] GetFormattedStatus_old(TimeSpan interval)
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
        //byte[] _payloadBytes;
        protected override void HandleRequest(EthernetMessage request)
        {
            _viewItem = new ViewItem<byte[]>(request.PayloadBytes, 5000);
            
            //_payloadBytes = request.PayloadBytes; // for viewer
            var ls = _digitalConverter.DownstreamConvert(request.PayloadBytes);
            ushort[] scan = ls as ushort[];
            System.Diagnostics.Debug.Assert(scan != null);
            _digitalWriter.WriteSingleScan(scan);
            //lock (_viewerItemist)
            //{
            //    for (int ch = 0; ch < scan.Length; ch++)
            //    {
            //        _viewerItemist[ch] = new ViewItem<UInt16>(scan[ch], timeToLiveMs: 5000);
            //    }
            //}
        }
    }
}

