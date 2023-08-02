using System.Collections.Generic;
using System.Net;
using NUnit.Framework;
using UeiBridge;
using UeiBridge.Library;

namespace UeiBridgeTest
{
    [TestFixture]
    class BuilderTests
    {
        //[Test]
        //public void BuildSimuDeviceList()
        //{
        //    var c2 = Config2.LoadConfigFromFile(new System.IO.FileInfo("UeiSettings2.simu.config"));

        //    ProgramObjectsBuilder programBuilder = new ProgramObjectsBuilder(c2);

        //    List<UeiDeviceInfo> deviceList = UeiBridge.Program.BuildLinearDeviceList(new List<string>(new string[] { "simu://" }));

        //    programBuilder.CreateDeviceManagers(deviceList);

        //    Assert.That(programBuilder.PerDeviceObjectsList.Count, Is.EqualTo(1));

        //    programBuilder.Dispose();
        //}

        [Test]
        public void CubeUriToIpAddressTest()
        {
            IPAddress ip1 = UeiBridge.Library.StaticMethods.CubeUrlToIpAddress("pdna://192.168.100.2/");
            byte[] bytes1 = ip1.GetAddressBytes();
            IPAddress ip2 = UeiBridge.Library.StaticMethods.CubeUrlToIpAddress("simu://");
            IPAddress ip3 = UeiBridge.Library.StaticMethods.CubeUrlToIpAddress("ddd");

            Assert.Multiple(() => 
            { 
                Assert.That(bytes1[0], Is.EqualTo(192)); 
                Assert.That(bytes1[3], Is.EqualTo(2));
                Assert.That(ip2, Is.Null);
                Assert.That(ip3, Is.Null);
            });
        }
    }
}
