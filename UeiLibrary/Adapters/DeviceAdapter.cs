using UeiDaq;

namespace UeiBridge.Library
{
    public class DeviceAdapter : IDevice
    {
        private Device _device;

        public DeviceAdapter(Device device)
        {
            this._device = device;
        }

        public Range[] GetAORanges()
        {
            return this._device.GetAORanges();
        }
    }
}