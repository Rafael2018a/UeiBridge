﻿using System;
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
        public override IConvert AttachedConverter => _attachedConverter;
        readonly IConvert _attachedConverter;
        public DIO430OutputDeviceManager()
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
                CloseDevice(); // if needed
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

    }
}
