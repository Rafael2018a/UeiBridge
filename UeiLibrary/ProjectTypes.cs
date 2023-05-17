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
        public string CubeUrl { get; set; }

        public UeiDeviceAdapter( UeiDaq.Device ueiDevice)
        {
            this.DeviceName = ueiDevice.GetDeviceName();
            this.DeviceSlot = ueiDevice.GetIndex();
        }

        public UeiDeviceAdapter(string cubeurl, string deviceName, int deviceSlot )
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
    public interface IReaderAdapter<ReadType> : IDisposable
    {
        ReadType EndRead(IAsyncResult ar);
        IAsyncResult BeginRead(int minLen, AsyncCallback readerCallback, int channel);
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
