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
        //private List<ViewItem<ushort>> _viewerItemist;
        private ViewItem<byte[]> _viewItem;
        private Session _ueiSession;
        private DIO403Setup _thisDeviceSetup;
        private List<byte> _scanMask = new List<byte>();
        
        private const int _maxNumberOfChannels = 6; // fixed. by device spec.

        public DIO403OutputDeviceManager(DeviceSetup setup, IWriterAdapter<UInt16[]> digitalWriter, UeiDaq.Session session) : base(setup)
        {
            this._digitalWriter = digitalWriter;
            this._ueiSession = session;
            this._thisDeviceSetup = setup as DIO403Setup;
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
            //_scanMask = new List<byte>(new byte[_maxNumberOfChannels]);
            for (int i = 0; i < _maxNumberOfChannels; i++)
            {
                _scanMask.Add(0);
            }
            foreach (Channel ch in _ueiSession.GetChannels())
            {
                _scanMask[ch.GetIndex()] = 0xff;
            }

            string res = _ueiSession.GetChannel(0).GetResourceName();
            string localpath = (new Uri(res)).LocalPath;
            EmitInitMessage($"Init success: {DeviceName}. As {localpath}. Listening on {_thisDeviceSetup.LocalEndPoint.ToIpEp()}"); // { noOfCh} output channels

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

        protected override void HandleRequest(EthernetMessage request)
        {
            if (request.PayloadBytes.Length < (_thisDeviceSetup.IOChannelList.Count))
            {
                _logger.Warn($"Incoming message too short. {request.PayloadBytes.Length} while expecting {_thisDeviceSetup.IOChannelList.Count}. rejected");
                return;
            }
            _viewItem = new ViewItem<byte[]>(request.PayloadBytes, 5000);

            byte[] distilledBuffer = new byte[_ueiSession.GetNumberOfChannels()];
            for (int ch = 0; ch < _ueiSession.GetNumberOfChannels(); ch++)
            {
                int i = _ueiSession.GetChannel(ch).GetIndex();
                distilledBuffer[ch] = request.PayloadBytes[i];
            }

            UInt16[] buffer16 = _digitalConverter.DownstreamConvert(distilledBuffer);
            System.Diagnostics.Debug.Assert(buffer16 != null);
            _digitalWriter.WriteSingleScan(buffer16);

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

