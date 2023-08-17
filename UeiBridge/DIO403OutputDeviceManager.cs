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
        private ViewItem<byte[]> _viewItem;
        //private ISession _ueiSession;
        private DIO403Setup _thisSetup;
        private List<byte> _scanMask = new List<byte>();
        
        //private const int _maxNumberOfChannels = 6; // fixed. by device spec.

        public DIO403OutputDeviceManager(DIO403Setup setup, ISession session) : base(setup)
        {
            this._digitalWriter = session.GetDigitalWriter();
            this._ueiSession = session;
            this._thisSetup = setup;// as DIO403Setup;
        }
        public DIO403OutputDeviceManager() { }// must have default c-tor

        public override void Dispose()
        {
            _digitalWriter?.Dispose();

            _ueiSession.Dispose();

            base.TerminateMessageLoop();

        }

        public override bool OpenDevice()
        {
            int numOfCh = _thisSetup.IOChannelList.Count;
            // build scan-mask
            byte[] ba = new byte[numOfCh];
            Array.Clear(ba, 0, ba.Length);
            _scanMask = new List<byte>(ba);
            //for (int i = 0; i < numOfCh; i++)
            //{
            //    _scanMask.Add(0);
            //}
            foreach (IChannel ch in _ueiSession.GetChannels())
            {
                _scanMask[ch.GetIndex()] = 0xff;
            }

            //string res = _ueiSession.GetChannel(0).GetResourceName();
            //string localpath = (new Uri(res)).LocalPath;
            EmitInitMessage($"Init success: {DeviceName}. Listening on {_thisSetup.LocalEndPoint.ToIpEp()}"); 

            Task.Factory.StartNew(() => OutputDeviceHandler_Task());
            _isDeviceReady = true;
            return _isDeviceReady;
        }

        public override string[] GetFormattedStatus(TimeSpan interval)
        {
            StringBuilder sb = new System.Text.StringBuilder("Output bits: ");
            if (null == _viewItem?.ReadValue)
            {
                return null;
            }
            if (_viewItem.TimeToLive > TimeSpan.Zero)
            {
                _viewItem.DecreaseTimeToLive( interval);
                for (int i = 0; i < _viewItem.ReadValue.Length; i++)
                {
                    if (_scanMask[i] > 0)
                    {
                        sb.Append(Convert.ToString(_viewItem.ReadValue[i], 2).PadLeft(8, '0'));
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
            if (request.PayloadBytes.Length < (_thisSetup.IOChannelList.Count))
            {
                _logger.Warn($"Incoming message too short. {request.PayloadBytes.Length} while expecting {_thisSetup.IOChannelList.Count}. rejected");
                return;
            }
            _viewItem = new ViewItem<byte[]>(request.PayloadBytes, TimeSpan.FromSeconds(5));

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

