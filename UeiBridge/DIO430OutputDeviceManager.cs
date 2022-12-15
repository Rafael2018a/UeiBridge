using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UeiDaq;

namespace UeiBridge
{
    
    class DIO430OutputDeviceManager: OutputDevice
    {
        public override string DeviceName =>  "DIO-430 not yet ready";
        string _channelsString;
        protected override IConvert AttachedConverter => _attachedConverter;

        //protected override string ChannelsString => _channelsString;

        public override string InstanceName => throw new NotImplementedException();

        readonly IConvert _attachedConverter;
        public DIO430OutputDeviceManager( DeviceSetup setup): base(setup)
        {
            _channelsString = "Do0";
            _attachedConverter = StaticMethods.CreateConverterInstance(DeviceName);
        }

        protected override void HandleRequest(EthernetMessage request)
        {
            throw new NotImplementedException();
        }

        protected virtual void HandleRequest(DeviceRequest dr)
        {
            // init session, if needed.
            // =======================
            if ((null == _deviceSession) || (_caseUrl != dr.CaseUrl))
            {
                
                //OpenDevice(dr, DeviceName);
            }

            // write to device
            // ===============
            UInt32[] req = dr.RequestObject as UInt32[];
            System.Diagnostics.Debug.Assert(req != null);// dr.RequestObject.GetType() == typeof(UInt32));
            //_writer.WriteSingleScanUInt32(  req );

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
