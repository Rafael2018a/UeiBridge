using System.Net;
using UeiBridge.Library;

namespace UeiBridgeSetup.ViewModels
{
    /// <summary>
    /// This model represents a uei-device which is laid in a specific cube-slot.
    /// </summary>
    public class DeviceSetupViewModel
    {
        public int SlotNumber => ThisDeviceSetup.SlotNumber;
        public IPAddress EnclosingCubeAddress { get; private set; }
        public int GetCubeId() 
        {
            return ThisDeviceSetup.GetCubeId();
        }
        public string DeviceName => ThisDeviceSetup.DeviceName;
        public DeviceSetup ThisDeviceSetup { get; private set; }

        public DeviceSetupViewModel(IPAddress cubeIp, DeviceSetup devSetup)
        {
            this.ThisDeviceSetup = devSetup;
            this.EnclosingCubeAddress = cubeIp;
            this._deviceDesc = DeviceMap2.GetDeviceDesc( ThisDeviceSetup.DeviceName);
        }
        string _deviceDesc;
        public string DeviceDesc => _deviceDesc;

    }
}