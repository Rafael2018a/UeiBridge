namespace UeiBridgeSetup.ViewModels
{
    public class UeiDevice
    {
        public string DeviceName { get; set; }
        public string DeviceDesc { get; set; }

        public UeiDevice(string deviceName, string deviceDesc)
        {
            DeviceName = deviceName;
            DeviceDesc = deviceDesc;
        }
    }
}