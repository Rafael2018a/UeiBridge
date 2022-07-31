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
        public DIO430OutputDeviceManager()
        {
            _deviceName = "DIO-430";
            _channelsString = "Do0";
        }
        public override void HandleRequest(DeviceRequest dr)
        {
            // init session, if needed.
            // =======================
            if ((null == _deviceSession) || (_caseUrl != dr.CaseUrl))
            {
                CloseDevice(); // if needed
                OpenDevice(dr);
            }

            // write to device
            // ===============

            UInt32[] req = dr.RequestObject as UInt32[];
            System.Diagnostics.Debug.Assert(req != null);// dr.RequestObject.GetType() == typeof(UInt32));
            _writer.WriteSingleScanUInt32(  req );

        }

    }
}
