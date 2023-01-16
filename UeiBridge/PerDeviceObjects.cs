using UeiDaq;
/// <summary>
/// All files in project might refer to this file.
/// Types in this file might NOT refer to types in any other file.
/// </summary>
namespace UeiBridge
{
    class PerDeviceObjects
    {
        public InputDevice InputDeviceManager { get; private set; }
        public OutputDevice OutputDeviceManager { get; private set; }
        public UdpReader UdpReader { get; private set; }
        public UdpWriter UdpWriter { get; private set; }
        public SL508Session SerialSession { get; set; }

        public PerDeviceObjects(InputDevice inputDevice, UdpWriter udpWriter)
        {
            InputDeviceManager = inputDevice;
            UdpWriter = udpWriter;
        }
        public void NewObjects(InputDevice inputDevice, UdpWriter udpWriter)
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
        public void NewObjects(OutputDevice outputDevice, UdpReader udpReader)
        {
            System.Diagnostics.Debug.Assert(null == OutputDeviceManager);
            System.Diagnostics.Debug.Assert(null == UdpReader);
            OutputDeviceManager = outputDevice;
            UdpReader = udpReader;
        }
    }

}
