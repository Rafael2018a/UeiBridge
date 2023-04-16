using System.Net;
using UeiBridge.Library;

namespace UeiBridgeSetup.ViewModels
{
    /// <summary>
    /// This view-model represents a uei-devices which is laid in a specific cube-slot.
    /// </summary>
    public class UeiSlotViewModel // tbd. maybe the name should be UeiDeviceViewModel
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