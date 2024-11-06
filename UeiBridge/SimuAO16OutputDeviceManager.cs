using UeiBridge.CubeSetupTypes;
using UeiBridge.DevManagers;
using UeiBridge.Library;

namespace UeiBridge
{
    public class SimuAO16OutputDeviceManager : AnalogOutDeviceManager
    {
        public override string DeviceName => DeviceMap2.SimuAO16Literal;
        //public SimuAO16OutputDeviceManager(AnalogOutputDeviceSetup deviceSetup, ISession session) : base(deviceSetup, session, false)
        //{
        //}

        public SimuAO16OutputDeviceManager(AnalogOutDeviceSetup deviceSetup1, ISession session, bool isBlockSensorActive) : base(deviceSetup1, session, isBlockSensorActive)
        {
        }

        public SimuAO16OutputDeviceManager()        {        }

    }
}