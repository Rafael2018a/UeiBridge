namespace UeiBridgeSetup.ViewModels
{
    public class UeiSlot
    {
        private int _slotNumber;
        private UeiDevice _deviceInSlot;

        public int SlotNumber { get => _slotNumber; set => _slotNumber = value; }
        public string SlotString { get { return $"Slot {_slotNumber}"; } }
        public UeiDevice DeviceInSlot { get => _deviceInSlot; set => _deviceInSlot = value; }

        public UeiSlot(int slotNumber, UeiDevice deviceInSlot)
        {
            _slotNumber = slotNumber;
            _deviceInSlot = deviceInSlot;
        }
    }
}