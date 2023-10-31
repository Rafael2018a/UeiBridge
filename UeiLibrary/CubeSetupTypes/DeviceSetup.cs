﻿using System;
using System.Xml.Serialization;
using UeiBridge.Library;

/// <summary>
/// All classes in this file MUST NOT depend on any other module in the project
/// </summary>
namespace UeiBridge.CubeSetupTypes
{
    public class DeviceSetup : IEquatable<DeviceSetup>
    {
        [XmlElement(ElementName = "DeviceSlot")]
        public int SlotNumber;
        public string DeviceName;
        public EndPoint LocalEndPoint;
        public EndPoint DestEndPoint;
        [XmlIgnore]
        public int SamplingInterval => 100; // ms
        [XmlIgnore]
        public string CubeUrl { get; set; }
        //[XmlIgnore]
        public int GetCubeId() // lsb of cube address
        {
            return StaticMethods.GetCubeId(CubeUrl);
        }
        public DeviceSetup(EndPoint localEndPoint, EndPoint destEndPoint, UeiDeviceInfo device)
        {
            LocalEndPoint = localEndPoint;
            DestEndPoint = destEndPoint;
            DeviceName = device.DeviceName;
            SlotNumber = device.DeviceSlot;
        }
        /// <summary>
        /// This c-tor for block sensor which does not have a 'uei device'
        /// </summary>
        public DeviceSetup(EndPoint localEndPoint, EndPoint destEndPoint, string deviceName)
        {
            LocalEndPoint = localEndPoint;
            DestEndPoint = destEndPoint;
            DeviceName = deviceName;
        }
        protected DeviceSetup()
        {
        }

        public bool Equals(DeviceSetup other)
        {
            //            public int SlotNumber;
            //public string DeviceName;
            //public EndPoint LocalEndPoint;
            //public EndPoint DestEndPoint;
            bool f1 = SlotNumber == other.SlotNumber;
            bool f2 = DeviceName == other.DeviceName;
            bool f3 = LocalEndPoint == null ? true : LocalEndPoint.Equals(other.LocalEndPoint);
            bool f4 = DestEndPoint == null ? true : DestEndPoint.Equals(other.DestEndPoint);

            return f1 && f2 && f3 && f4;
        }
        public string GetInstanceName()
        {
            return $"{DeviceName}/Cube{GetCubeId()}/Slot{SlotNumber}";
        }
        public UeiDeviceInfo GetDeviceInfo()
        {
            return new UeiDeviceInfo(CubeUrl, SlotNumber, DeviceName);
        }
    }
}
