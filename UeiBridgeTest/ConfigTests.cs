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
        [Test]
        [Category("Config")]
        public void NoConfigTest()
        {
            Config2.Reset();
            if (Config2.IsConfigFileExist())
                System.IO.File.Delete(Config2.SettingsFilename);
            DeviceSetup ds = Config2.Instance.GetSetupEntryForDevice("simu://", 2);
            Assert.That(ds, Is.Null);
        }
        [Test]
        [Category("Config")]
        public void LoadConfigTest()
        {
            if (Config2.IsConfigFileExist())
                System.IO.File.Delete(Config2.SettingsFilename);

            if (!Config2.IsConfigFileExist())
            {
                Config2.Instance.BuildNewConfig(new string[] { "simu://" });
            }
            DeviceSetup ds = Config2.Instance.GetSetupEntryForDevice("simu://", 2);

            Assert.That(ds, Is.Not.Null);
            Assert.That(ds.SlotNumber, Is.EqualTo(2));
        }

    }
}
