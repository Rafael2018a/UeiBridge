namespace UeiBridge
{
    /// <summary>
    /// from the manual:
    /// ** 48-channel Digital I/O **
    /// </summary>
    class DIO403OutputDeviceManager : DioOutputDeviceManager
    {
        public DIO403OutputDeviceManager()
        {
            _deviceName = "DIO-403";
            _channelsString = "Do0:2";
            _attachedConverter = StaticMethods.CreateConverterInstance( _deviceName);
        }
    }
}

