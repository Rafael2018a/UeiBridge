using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UeiBridge.CubeSetupTypes;
using UeiBridge.Interfaces;
using UeiBridge.Library;
using UeiBridge.Types;
using UeiDaq;
using static System.Collections.Specialized.BitVector32;

namespace UeiBridge.DevManagers
{
    public class DIO64OutputDeviceManager : OutputDevice
    {
        private DIO64Setup _thisSetup;
        private DigitalConverter _digitalConverter = new DigitalConverter();
        private IWriterAdapter<UInt16[]> _digitalWriter;
        public DIO64OutputDeviceManager()
        {
        }

        public DIO64OutputDeviceManager( DIO64Setup deviceSetup, ISession session) : base(deviceSetup)
        {
            this._digitalWriter = session.GetDigitalWriter();
            this._thisSetup = deviceSetup;  
            this._ueiSession=session;
        }

        public override string DeviceName => DeviceMap2.SimuDIO64Literal;


        public override void Dispose()
        {
            
        }

        public override string[] GetFormattedStatus(TimeSpan interval)
        {
            return null;
        }

        public override bool OpenDevice()
        {
            int numOfCh = 4;
            // build scan-mask
            byte[] ba = new byte[numOfCh];
            Array.Clear(ba, 0, ba.Length);
            //_scanMask = new List<byte>(ba);
            //for (int i = 0; i < numOfCh; i++)
            //{
            //    _scanMask.Add(0);
            //}
            //foreach (IChannel ch in _ueiSession.GetChannels())
            //{
            //    _scanMask[ch.GetIndex()] = 0xff;
            //}

            //string res = _ueiSession.GetChannel(0).GetResourceName();
            //string localpath = (new Uri(res)).LocalPath;
            EmitInitMessage($"Init success: {DeviceName}. Listening on {_thisSetup.LocalEndPoint.ToIpEp()}");

            Task.Factory.StartNew(() => OutputDeviceHandler_Task());
            _isDeviceReady = true;
            return _isDeviceReady;


        }

        protected override void HandleRequest(EthernetMessage request)
        {

            byte[] distilledBuffer = new byte[_ueiSession.GetNumberOfChannels()];
            for (int ch = 0; ch < _ueiSession.GetNumberOfChannels(); ch++)
            {
                int i = _ueiSession.GetChannel(ch).GetIndex();
                distilledBuffer[ch] = request.PayloadBytes[i];
            }

            UInt16[] buffer16 = _digitalConverter.DownstreamConvert(distilledBuffer);
            System.Diagnostics.Debug.Assert(buffer16 != null);
            _digitalWriter.WriteSingleScan(buffer16);

        }
    }
}
