using System.Net;
using UeiBridge.Library;

namespace UeiBridgeSetup.ViewModels
{
    /// <summary>
    /// This model represents a uei-device which is laid in a specific cube-slot.
    /// </summary>
    public class PhysicalDevice 
    {
        //private IPAddress _cubeIPAddress;
        //private DeviceSetup _deviceSetup;

        public int SlotNumber => ThisDeviceSetup.SlotNumber;
        public IPAddress EnclosingCubeAddress { get; private set; }
        public int GetCubeId() 
        {
            return StaticMethods.GetCubeId(this.EnclosingCubeAddress); 
            
        }
        public string DeviceName => ThisDeviceSetup.DeviceName;
        public DeviceSetup ThisDeviceSetup { get; private set; }// => this._deviceSetup;
        public PhysicalDevice(IPAddress cubeIp, DeviceSetup dev)
        {
            this.ThisDeviceSetup = dev;
            this.EnclosingCubeAddress = cubeIp;

            //CubeId = cubeIp.GetAddressBytes()[3];
        }

        public string DeviceDesc
        {
            get
            {
                return DeviceMap2.GetDeviceDesc( ThisDeviceSetup.DeviceName);
            }
            set { }
        }

    }
}