using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using UeiBridge;
using UeiBridge.Types;
using UeiBridge.Library;

namespace UeiBridgeTest
{
    [TestFixture]
    class BuilderTests
    {
        [Test]
        public void BuildSimuDeviceList()
        {
            ProgramObjectsBuilder programBuilder = new ProgramObjectsBuilder();
            if (!Config2.IsConfigFileExist())
                Config2.Instance.BuildNewConfig(new string[] { "simu://" });

            List<DeviceEx> deviceList = UeiBridge.Program.BuildDeviceList(new List<string>( new string[] { "simu://" }));

            programBuilder.CreateDeviceManagers( deviceList);

            Assert.That(programBuilder.DeviceManagers.Count, Is.EqualTo(1));
        }

        [Test]
        public void ParseDevieUrl()
        {
            var ip1 = UeiBridge.Library.StaticMethods.GetIpAddressFromUrl("pdna://192.168.100.2/");
            byte[] bytes1 = ip1.GetAddressBytes();
            var ip2 = UeiBridge.Library.StaticMethods.GetIpAddressFromUrl("simu://");
            byte[] bytes2 = ip2.GetAddressBytes();

            Assert.Multiple(() => 
            { 
                Assert.That(bytes1[0], Is.EqualTo(192)); 
                Assert.That(bytes1[3], Is.EqualTo(2)); 
                Assert.That(bytes2[2], Is.EqualTo(255));
            });
        }

        [Test]
        public void buildtime()
        {
            var v = System.Reflection.Assembly.GetExecutingAssembly();
            System.IO.FileInfo fi = new System.IO.FileInfo(v.Location);
            var dt = fi.CreationTime;
        }
    }
}
