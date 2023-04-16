using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UeiBridge;
using UeiBridge.Library;
using NUnit.Framework;

namespace UeiBridgeTest
{
    [TestFixture]
    class ConfigTests
    {
        //[Test]
        //public void NoConfigTest()
        //{
        //    Config2.Reset();
        //    if (Config2.IsConfigFileExist())
        //        System.IO.File.Delete(Config2.SettingsFilename);
        //    DeviceSetup ds = Config2.Instance.GetSetupEntryForDevice("simu://", 2);
        //    Assert.That(ds, Is.Null);
        //}
        //[Test]
        //public void LoadConfigTest()
        //{
        //    if (Config2.IsConfigFileExist())
        //        System.IO.File.Delete(Config2.SettingsFilename);

        //    if (!Config2.IsConfigFileExist())
        //    {
        //        Config2.Instance.BuildNewConfig(new string[] { "simu://" });
        //    }
        //    DeviceSetup ds = Config2.Instance.GetSetupEntryForDevice("simu://", 2);

        //    Assert.That(ds, Is.Not.Null);
        //    Assert.That(ds.SlotNumber, Is.EqualTo(2));
        //}

        [Test]
        public void NoConfigFile()
        {
            Config2 c2 = new Config2("nofile.config");
            Assert.That(c2.UeiCubes.Count, Is.EqualTo(0));
        }
        [Test]
        public void BuiildNewConfigFile()
        {
            System.IO.File.Delete("newfile.config");
            string url = "simu://";
            CubeSetup cs = new CubeSetup(url);
            Config2 c2 = new Config2(new List<CubeSetup> { cs });
            c2.SaveAs("newfile.config");
        }

        [Test]
        public void CubeSetupTest1()
        {
            List<UeiDeviceAdapter> devList = new List<UeiDeviceAdapter>();
            devList.Add(new UeiDeviceAdapter("devicename1", 51));
            CubeSetup cs = new CubeSetup(devList, "<unknown-url>");
            Assert.That( cs.DeviceSetupList.Count, Is.EqualTo(0));
        }
        [Test]
        public void CubeSetupTest2()
        {
            List<UeiDeviceAdapter> devList = new List<UeiDeviceAdapter>();
            devList.Add(new UeiDeviceAdapter("AO-308", 101));
            CubeSetup cs = new CubeSetup(devList, "<unknown-url>");
            Assert.That( cs.DeviceSetupList.Count, Is.EqualTo(1));
            Assert.That(cs.DeviceSetupList[0], Is.Not.Null);
        }

        // 
    }
}
