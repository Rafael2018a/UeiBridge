using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using UeiBridge.Library;
//using UeiBridgeSetup;
//using UeiBridgeSetup.ViewModels;
using CubeDesign;
using CubeDesign.ViewModels;

namespace UeiBridgeTest
{
    [TestFixture]
    class ViewModelTests
    {
        [Test]
        public void EmptyEndPointVM_Test1()
        {
            EndPointViewModel vm = new EndPointViewModel( EndPointLocation.Dest, null);

            Assert.Multiple(() =>
            {
                Assert.That(vm.Address, Is.EqualTo("0.0.0.0"));
                Assert.That(vm.IpPort, Is.EqualTo(0));
            });
        }
        [Test]
        public void EndPointVM2_Test()
        {
            EndPointViewModel vm = new EndPointViewModel(EndPointLocation.Dest, new UeiBridge.Library.EndPoint("8.8.8.8", 5050));
            
            Assert.Multiple(() =>
            {
                Assert.That(vm.Address, Is.EqualTo("8.8.8.8"));
                Assert.That(vm.IpPort, Is.EqualTo(5050));
            });
        }

        [Test]
        public void CubeSetupVM1_Test()
        {
            var devList = UeiBridge.Program.BuildLinearDeviceList(new List<string> { "simu://" });

            var resList = devList.Select(d => new UeiDeviceInfo("simu://", d.DeviceSlot, d.DeviceName));// as List<UeiDeviceAdapter>;
            List<UeiDeviceInfo> l = new List<UeiDeviceInfo>(resList);

            CubeSetup cs = new CubeSetup(l);
            CubeSetupViewModel cube = new CubeSetupViewModel(cs, false);
            Assert.That(cube.CubeAddress, Is.Null);
        }
        //[Test]
        //public void SystemSetupVM_Test()
        //{
        //    if (System.IO.File.Exists(Config2.DafaultSettingsFilename))
        //    {
        //        Config2 c2 = Config2.LoadConfigFromFile( new System.IO.FileInfo( Config2.DafaultSettingsFilename));
        //        SystemSetupViewModel sysVM = new SystemSetupViewModel( c2);
        //        Assert.That(sysVM.SlotList.Count, Is.GreaterThan(0));
        //    }
        //}
    }
}
