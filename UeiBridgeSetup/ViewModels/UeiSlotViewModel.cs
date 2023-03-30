using System.Net;
using UeiBridge.Library;

namespace UeiBridgeSetup.ViewModels
{
    public class UeiSlotViewModel
    {
        private IPAddress _cubeIPAddress;
        private DeviceSetup _deviceSetup;

        public int SlotNumber => _deviceSetup.SlotNumber;
        public IPAddress CubeIPAddress { get => _cubeIPAddress; set => _cubeIPAddress = value; }
        public string DeviceName => _deviceSetup.DeviceName;
        public DeviceSetup ThisDeviceSetup => this._deviceSetup;
        public UeiSlotViewModel(IPAddress cubeIp, DeviceSetup dev)
        {
            this._deviceSetup = dev;
            this._cubeIPAddress = cubeIp;
        }
    }
}