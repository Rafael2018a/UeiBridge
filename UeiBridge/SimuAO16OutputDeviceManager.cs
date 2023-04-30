using UeiBridge.Library;

namespace UeiBridge
{
    public class SimuAO16OutputDeviceManager : AO308OutputDeviceManager
    {
        public override string DeviceName => DeviceMap2.SimuAO16Literal;
        public SimuAO16OutputDeviceManager(DeviceSetup deviceSetup, IWriterAdapter<double[]> analogWriter, UeiDaq.Session session) : base(deviceSetup, analogWriter, session)
        {
        }

        public SimuAO16OutputDeviceManager()
        {
        }
    }
}