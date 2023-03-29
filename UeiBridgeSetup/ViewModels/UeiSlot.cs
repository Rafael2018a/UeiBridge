using System.Net;

namespace UeiBridgeSetup.ViewModels
{
    public class UeiSlot
    {
        private int _slotNumber;
        private UeiDevice _deviceInSlot;
        private IPAddress _cubeIPAddress;

        public int SlotNumber { get => _slotNumber; set => _slotNumber = value; }
        //public string SlotNumberString { get { return $"Slot {_slotNumber}"; } }
        public UeiDevice DeviceInSlot { get => _deviceInSlot; set => _deviceInSlot = value; }
        public IPAddress CubeIPAddress { get => _cubeIPAddress; set => _cubeIPAddress = value; }
        public UeiSlot(IPAddress cubeIPAddress, int slotNumber, UeiDevice deviceInSlot )
        {
            _slotNumber = slotNumber;
            _deviceInSlot = deviceInSlot;
            _cubeIPAddress = cubeIPAddress;
        }
    }
}