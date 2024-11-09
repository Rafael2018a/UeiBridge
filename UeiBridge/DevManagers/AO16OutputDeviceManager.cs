using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UeiBridge.CubeSetupTypes;
using UeiBridge.Library;

namespace UeiBridge.DevManagers
{
    public class AO16OutputDeviceManager: AnalogOutDeviceManager
    {
        public AO16OutputDeviceManager()
        {
        }

        public AO16OutputDeviceManager(AO16Setup deviceSetup1, ISession session, bool isBlockSensorActive) : base(deviceSetup1, session, isBlockSensorActive)
        {
        }

        public override string DeviceName => DeviceMap2.SimuAO16Literal;

    }
}
