using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UeiDaq;

namespace UeiBridge.Library
{
    class ProjectTypes
    {
    }

    public class UeiDeviceAdapter
    {
        public string DeviceName { get; private set; }
        public int DeviceSlot { get; private set; }
        public UeiDeviceAdapter( UeiDaq.Device ueiDevice)
        {
            this.DeviceName = ueiDevice.GetDeviceName();
            this.DeviceSlot = ueiDevice.GetIndex();
        }

        public UeiDeviceAdapter(string deviceName, int deviceSlot)
        {
            DeviceName = deviceName;
            DeviceSlot = deviceSlot;
        }
    }

    public interface IWriterAdapter<T>: IDisposable
    {
        void WriteSingleScan(T scan);
        //int NumberOfChannels { get; }
        //UeiDaq.Session OriginSession { get; }
    }

    public struct DeviceEx
    {
        public UeiDaq.Device PhDevice { get; private set; }
        public string CubeUrl { get; private set; }
        public DeviceEx(Device device, string cubeUrl)
        {
            PhDevice = device;
            CubeUrl = cubeUrl;
        }
    }


}
