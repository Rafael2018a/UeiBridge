using UeiBridge.CubeSetupTypes;
using UeiBridge.Library;

namespace UeiBridge
{
    public class SimuAO16OutputDeviceManager : AO308OutputDeviceManager
    {
        public override string DeviceName => DeviceMap2.SimuAO16Literal;
        public SimuAO16OutputDeviceManager(AO308Setup deviceSetup, ISession session) : base(deviceSetup, session, false)
        {
        }

        public SimuAO16OutputDeviceManager()
        {
        }
    }
}