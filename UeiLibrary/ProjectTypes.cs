using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UeiDaq;

namespace UeiBridge.Library
{
    class ProjectTypes
    {
    }

    public class UeiDeviceInfo
    {
        public string DeviceName { get; private set; }
        public int DeviceSlot { get; private set; }
        public string CubeUrl { get; set; }
        public int CubeId
        {
            get
            {
                IPAddress ipa = Config2.CubeUriToIpAddress(this.CubeUrl);
                if (null != ipa)
                {
                    return ipa.GetAddressBytes()[3];
                }
                else
                {
                    return -1;
                }
            }
        }
        public UeiDeviceInfo( UeiDaq.Device ueiDevice) // todo. add url.
        {
            this.DeviceName = ueiDevice.GetDeviceName();
            this.DeviceSlot = ueiDevice.GetIndex();
            
        }

        public UeiDeviceInfo(string cubeurl, string deviceName, int deviceSlot )
        {
            CubeUrl = cubeurl;
            DeviceSlot = deviceSlot;
            DeviceName = deviceName;
        }
    }

    public interface IWriterAdapter<T>: IDisposable
    {
        void WriteSingleScan(T scan);
    }
    public interface IReadAdapter<T>: IDisposable
    {

    }

    public struct DeviceEx1
    {
        public UeiDaq.Device PhDevice { get; private set; }
        public string CubeUrl { get; private set; }
        public DeviceEx1(Device device, string cubeUrl)
        {
            PhDevice = device;
            CubeUrl = cubeUrl;
        }
    }


}
