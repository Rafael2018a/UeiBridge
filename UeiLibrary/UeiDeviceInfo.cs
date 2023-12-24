﻿using System.Net;

namespace UeiBridge.Library
{
    public class UeiDeviceInfo
    {
        public string DeviceName { get; private set; }
        public int DeviceSlot { get; private set; }
        public string CubeUrl { get; set; }
        public int CubeId { get; private set; }
        public static int SimuCubeId {get; } = 101; // const
        /// <summary>
        /// UeiDeviceInfo does NOT depends on UeiDaq namespace types
        /// </summary>
        public UeiDeviceInfo(string cubeUrl, int deviceSlot , string deviceName)
        {
            CubeUrl = cubeUrl;
            DeviceSlot = deviceSlot;
            DeviceName = deviceName;

            if (null==cubeUrl) // block sensor
            {
                CubeId = -1;
            }
            else if (cubeUrl.ToLower().StartsWith("simu"))
            {
                CubeId = SimuCubeId;
            }
            else
            {
                IPAddress ipa = UeiCube.CubeUriToIpAddress(this.CubeUrl);
                CubeId = (null != ipa) ? ipa.GetAddressBytes()[3] : -1;
            }
        }
    }

    //public struct DeviceEx1
    //{
    //    public UeiDaq.Device PhDevice { get; private set; }
    //    public string CubeUrl { get; private set; }
    //    public DeviceEx1(Device device, string cubeUrl)
    //    {
    //        PhDevice = device;
    //        CubeUrl = cubeUrl;
    //    }
    //}




}
