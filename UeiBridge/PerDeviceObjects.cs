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
        public UdpWriter UdpWriter { get; set; }
        //public SL508Session SerialSession { get; set; }
        public PerDeviceObjects(string deviceName, int slotNumber, string cubeUrl)
        {
            this.DeviceName = deviceName;
            this.SlotNumber = slotNumber;
            this.CubeUrl = cubeUrl;
        }
        public PerDeviceObjects( DeviceEx deviceEx)//, OutputDevice outDevice, UdpReader reader)
        {
            this.DeviceName = deviceEx.PhDevice.GetDeviceName();
            this.SlotNumber = deviceEx.PhDevice.GetIndex();
            this.CubeUrl = deviceEx.CubeUrl;
            //this.OutputDeviceManager = outDevice;
            //this.UdpReader = reader;
        }
        public void Update(InputDevice inputDevice, UdpWriter udpWriter, int slotNumber)
        {
            System.Diagnostics.Debug.Assert( inputDevice.DeviceName == this.DeviceName);
            System.Diagnostics.Debug.Assert(slotNumber == this.SlotNumber);
            this.InputDeviceManager = inputDevice;
            this.UdpWriter = udpWriter;
        }
        //public void Update(OutputDevice outputDevice, UdpReader udpReader, int slotNumber)
        //{
        //    System.Diagnostics.Debug.Assert(outputDevice.DeviceName == this.DeviceName);
        //    System.Diagnostics.Debug.Assert(slotNumber == this.SlotNumber);
        //    this.OutputDeviceManager = outputDevice;
        //    this.UdpReader = udpReader;

        //}
        //public void Update(SL508Session serialSession, int slotNumber)
        //{
        //    System.Diagnostics.Debug.Assert(slotNumber == this.SlotNumber);
        //    this.SerialSession = serialSession;
        //}
        //public PerDeviceObjects(InputDevice inputDevice, UdpWriter udpWriter, SL508Session serialSession)
        //{


        //    System.Diagnostics.Debug.Assert(inputDevice.DeviceName == DeviceName);
        //}
        //public PerDeviceObjects(OutputDevice outputDevice, UdpReader udpReader, SL508Session serialSession)
        //{
        //    this.SerialSession = serialSession;

        //    System.Diagnostics.Debug.Assert( outputDevice.DeviceName == DeviceName);
        //}
    }

}
