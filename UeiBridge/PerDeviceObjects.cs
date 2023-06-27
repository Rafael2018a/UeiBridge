using UeiDaq;
using UeiBridge.Library;

namespace UeiBridge
{
    /// <summary>
    /// This class holds objects that refer to specific card in uei cube.
    /// A card might have input or output manager or both
    /// reference to udp-reader or udp-writer is also included, (even thought each device manager have its own reference)
    /// </summary>
    public class PerDeviceObjects
    {
        public string DeviceName { get; private set; }
        public int SlotNumber { get; private set; }
        public string CubeUrl { get; private set; }
        public InputDevice InputDeviceManager { get;  set; }
        public OutputDevice OutputDeviceManager { get; set; }
        public PerDeviceObjects(string deviceName, int slotNumber, string cubeUrl)
        {
            this.DeviceName = deviceName;
            this.SlotNumber = slotNumber;
            this.CubeUrl = cubeUrl;
        }
        public PerDeviceObjects(UeiDeviceInfo deviceEx)//, OutputDevice outDevice, UdpReader reader)
        {
            this.DeviceName = deviceEx.DeviceName;
            this.SlotNumber = deviceEx.DeviceSlot;
            this.CubeUrl = deviceEx.CubeUrl;
        }
    }

}
