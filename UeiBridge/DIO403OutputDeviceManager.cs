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

        public DIO403OutputDeviceManager()
        {
            _channelsString = "Do0:2";
            _attachedConverter = StaticMethods.CreateConverterInstance( DeviceName);
        }

        public override string DeviceName => "DIO-403";
    }
}

