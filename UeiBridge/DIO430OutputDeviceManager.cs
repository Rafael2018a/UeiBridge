using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UeiDaq;

namespace UeiBridge
{
    
    class DIO430OutputDeviceManager: DioOutputDeviceManager
    {
        public override string DeviceName =>  "DIO-430";
        string _channelsString;
        public override IConvert AttachedConverter => _attachedConverter;

        protected override string ChannelsString => _channelsString;

        readonly IConvert _attachedConverter;
        public DIO430OutputDeviceManager( DeviceSetup setup): base(setup)
        {
            _channelsString = "Do0";
            _attachedConverter = StaticMethods.CreateConverterInstance(DeviceName);
        }


        protected override void HandleRequest(DeviceRequest dr)
        {
            // init session, if needed.
            // =======================
            if ((null == _deviceSession) || (_caseUrl != dr.CaseUrl))
            {
                CloseSession(); // if needed
                OpenDevice(dr, DeviceName);
            }

            // write to device
            // ===============
            UInt32[] req = dr.RequestObject as UInt32[];
            System.Diagnostics.Debug.Assert(req != null);// dr.RequestObject.GetType() == typeof(UInt32));
            _writer.WriteSingleScanUInt32(  req );

        }
        public override string GetFormattedStatus()
        {
            return "(not ready yet)";
        }

        public override void Dispose()
        {
            // tbd
            //_deviceSession.Stop();
            //_deviceSession.Dispose();
            //_deviceSession = null;
        }

        public override bool OpenDevice()
        {
            throw new NotImplementedException();
        }
    }
}
