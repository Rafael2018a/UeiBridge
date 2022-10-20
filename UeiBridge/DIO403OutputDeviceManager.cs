namespace UeiBridge
{
    /// <summary>
    /// from the manual:
    /// ** 48-channel Digital I/O **
    /// </summary>
    class DIO403OutputDeviceManager : DioOutputDeviceManager
    {
        public override IConvert AttachedConverter => _attachedConverter;
        readonly IConvert _attachedConverter;
        string _channelsString;
        public DIO403OutputDeviceManager()
        {
            _channelsString = "Do0:2";
            _attachedConverter = StaticMethods.CreateConverterInstance( DeviceName);
        }

        public override string DeviceName => "DIO-403";

        protected override string ChannelsString => _channelsString;

        public override void Dispose()
        {
            OutputDevice deviceManager = ProjectRegistry.Instance.OutputDevicesMap[DeviceName];
            DeviceRequest dr = new DeviceRequest(OutputDevice.CancelTaskRequest, "");
            deviceManager.Enqueue(dr);
            System.Threading.Thread.Sleep(100);
            CloseDevice();
        }
    }
}

