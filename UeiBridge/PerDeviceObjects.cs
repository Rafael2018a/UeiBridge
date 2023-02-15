using UeiDaq;
/// <summary>
/// All files in project might refer to this file.
/// Types in this file might NOT refer to types in any other file.
/// </summary>
namespace UeiBridge
{
    class PerDeviceObjects
    {
        public InputDevice InputDeviceManager { get; set; }
        public OutputDevice OutputDeviceManager { get; set; }
        public UdpReader UdpReader { get; set; }
        public UdpWriter UdpWriter { get; set; }
        public SL508Session SerialSession { get; set; }
        //public BlockSensorManager BlockSensor { get; set; }
        public PerDeviceObjects()
        { }
        //public PerDeviceObjects( BlockSensorManager bsensor, UdpReader udpReader)
        //{
        //    this.BlockSensor = bsensor;
        //    this.UdpReader = udpReader;
        //}
        private PerDeviceObjects(InputDevice inputDevice, UdpWriter udpWriter)
        {
            InputDeviceManager = inputDevice;
            UdpWriter = udpWriter;
        }
        private void NewObjects(InputDevice inputDevice, UdpWriter udpWriter)
        {
            System.Diagnostics.Debug.Assert(InputDeviceManager == null);
            System.Diagnostics.Debug.Assert(UdpWriter == null);
            InputDeviceManager = inputDevice;
            UdpWriter = udpWriter;
        }

        public PerDeviceObjects(OutputDevice outputDevice, UdpReader udpReader)
        {
            OutputDeviceManager = outputDevice;
            UdpReader = udpReader;
        }
        private void NewObjects(OutputDevice outputDevice, UdpReader udpReader)
        {
            System.Diagnostics.Debug.Assert(null == OutputDeviceManager);
            System.Diagnostics.Debug.Assert(null == UdpReader);
            OutputDeviceManager = outputDevice;
            UdpReader = udpReader;
        }
    }

}
