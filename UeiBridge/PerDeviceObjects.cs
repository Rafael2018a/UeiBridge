using UeiDaq;
/// <summary>
/// All files in project might refer to this file.
/// Types in this file might NOT refer to types in any other file.
/// </summary>
namespace UeiBridge
{
    class PerDeviceObjects
    {
        public InputDevice _inputDeviceManager;
        public OutputDevice _outputDeviceManager;
        public UdpReader _udpReader;
        public UdpWriter _udpWriter;
        //UeiDaq.Session _serialDeviceSession;

        public Session SerialSession { get; set; }

        public PerDeviceObjects(InputDevice inputDevice, UdpWriter udpWriter)
        {
            _inputDeviceManager = inputDevice;
            _udpWriter = udpWriter;
        }
        public void NewObjects(InputDevice inputDevice, UdpWriter udpWriter)
        {
            System.Diagnostics.Debug.Assert(_inputDeviceManager == null);
            System.Diagnostics.Debug.Assert(_udpWriter == null);
            _inputDeviceManager = inputDevice;
            _udpWriter = udpWriter;
        }

        public PerDeviceObjects(OutputDevice outputDevice, UdpReader udpReader)
        {
            _outputDeviceManager = outputDevice;
            _udpReader = udpReader;
        }
        public void NewObjects(OutputDevice outputDevice, UdpReader udpReader)
        {
            System.Diagnostics.Debug.Assert(null == _outputDeviceManager);
            System.Diagnostics.Debug.Assert(null == _udpReader);
            _outputDeviceManager = outputDevice;
            _udpReader = udpReader;
        }
    }

}
