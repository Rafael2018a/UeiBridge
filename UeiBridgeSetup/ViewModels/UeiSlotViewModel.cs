using System.Net;
using UeiBridge.Library;

namespace UeiBridgeSetup.ViewModels
{
    /// <summary>
    /// This model represents a uei-device which is laid in a specific cube-slot.
    /// </summary>
    public class SlotDeviceModel 
    {
        //private IPAddress _cubeIPAddress;
        //private DeviceSetup _deviceSetup;

        public int SlotNumber => ThisDeviceSetup.SlotNumber;
        public IPAddress EnclosingCubeAddress { get; set; }
        public string DeviceName => ThisDeviceSetup.DeviceName;
        public DeviceSetup ThisDeviceSetup { get; set; }// => this._deviceSetup;
        public SlotDeviceModel(IPAddress cubeIp, DeviceSetup dev)
        {
            this.ThisDeviceSetup = dev;
            this.EnclosingCubeAddress = cubeIp;
        }
    }
}