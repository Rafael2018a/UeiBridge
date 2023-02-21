using UeiDaq;
namespace UeiBridge
{
    /// <summary>
    /// This class holds objects that refer to specific card in uei cube.
    /// A card might have input or output manager or both
    /// reference to udp-reader or udp-writer is also included, (even thought each device manager have its own reference)
    /// </summary>
    class PerDeviceObjects
    {
        public InputDevice InputDeviceManager { get; private set; }
        public OutputDevice OutputDeviceManager { get; private set; }
        public UdpReader UdpReader { get; private set; }
        public UdpWriter UdpWriter { get; private set; }
        public SL508Session SerialSession { get; set; }
        public PerDeviceObjects()
        { }
        public PerDeviceObjects(InputDevice inputDevice, UdpWriter udpWriter, SL508Session serialSession)
        {
            this.InputDeviceManager = inputDevice;
            this.UdpWriter = udpWriter;
            this.SerialSession = serialSession;
        }
        public PerDeviceObjects(OutputDevice outputDevice, UdpReader udpReader, SL508Session serialSession)
        {
            this.OutputDeviceManager = outputDevice;
            this.UdpReader = udpReader;
            this.SerialSession = serialSession;
        }
    }

}
