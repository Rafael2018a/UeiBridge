using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using UeiBridge;
using UeiBridge.Types;

namespace UeiBridgeTest
{
    [TestFixture]
    class BuilderTests
    {
        [Test]
        public void BuildSimuObjectTest()
        {
            ProgramObjectsBuilder programBuilder = new ProgramObjectsBuilder();
            if (!Config2.IsConfigFileExist())
                Config2.Instance.BuildNewConfig(new string[] { "simu://" });

            List<DeviceEx> deviceList = UeiBridge.Program.BuildDeviceList(new List<string>( new string[] { "simu://" }));

            programBuilder.CreateDeviceManagers( deviceList);

            Assert.That(programBuilder._deviceManagers.Count, Is.EqualTo(1));
        }
    }
}
