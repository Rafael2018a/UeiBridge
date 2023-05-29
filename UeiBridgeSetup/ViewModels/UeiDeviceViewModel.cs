namespace UeiBridgeSetup.ViewModels
{
    public class UeiDeviceViewModel
    {
        public string DeviceName { get; set; }
        public string DeviceDesc { get; set; }

        public UeiDeviceViewModel(string deviceName)
        {
            DeviceName = deviceName;
            //DeviceDesc = deviceDesc;
        }
    }
}