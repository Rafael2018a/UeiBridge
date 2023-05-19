using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using NUnit.Framework;
using UeiBridge;
using UeiBridge.Library;

namespace UeiBridgeTest
{
    [TestFixture]
    class BuilderTests
    {

        [SetUp]
        public void  SetUp()
        {
            Trace.Listeners.Add(new ConsoleTraceListener());
        }
        [Test]
        public void BuildSimuDeviceList()
        {
            var c2 = Config2.LoadConfigFromFile(new System.IO.FileInfo("UeiSettings2.simu.config"));

            ProgramObjectsBuilder programBuilder = new ProgramObjectsBuilder(c2);

            List<UeiDeviceInfo> deviceList = UeiBridge.Program.BuildDeviceList(new List<string>(new string[] { "simu://" }));

            programBuilder.CreateDeviceManagers(deviceList);

            Assert.That(programBuilder.PerDeviceObjectsList.Count, Is.EqualTo(1));

            programBuilder.Dispose();

            Trace.WriteLine("success");
        }

        [Test]
        public void ParseDevieUrl()
        {
            IPAddress ip1 = Config2.CubeUriToIpAddress("pdna://192.168.100.2/");
            byte[] bytes1 = ip1.GetAddressBytes();
            IPAddress ip2 = Config2.CubeUriToIpAddress("simu://");
            IPAddress ip3 = Config2.CubeUriToIpAddress("ddd");

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
