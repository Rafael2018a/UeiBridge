using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using UeiBridge.Library;
using UeiBridgeSetup;
using UeiBridgeSetup.ViewModels;

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
                Assert.That(vm.IpString, Is.EqualTo("0.0.0.0"));
                Assert.That(vm.IpPort, Is.EqualTo(0));
            });
        }
        [Test]
        public void EndPointVM2_Test()
        {
            EndPointViewModel vm = new EndPointViewModel(EndPointLocation.Dest, new UeiBridge.Library.EndPoint("8.8.8.8", 5050));
            
            Assert.Multiple(() =>
            {
                Assert.That(vm.IpString, Is.EqualTo("8.8.8.8"));
                Assert.That(vm.IpPort, Is.EqualTo(5050));
            });
        }

        [Test]
        public void CubeSetupVM1_Test()
        {
            var devList = UeiBridge.Program.BuildDeviceList("simu://");

            var resList = devList.Select(d => new UeiDeviceAdapter(d.PhDevice.GetDeviceName(), d.PhDevice.GetIndex()));// as List<UeiDeviceAdapter>;
            List<UeiDeviceAdapter> l = new List<UeiDeviceAdapter>(resList);

            CubeSetup cs = new CubeSetup(l, "simu://");
            CubeSetupViewModel cube = new CubeSetupViewModel(cs, false);
            Assert.That(cube.CubeAddress, Is.Null);
        }
        [Test]
        public void SystemSetupVM_Test()
        {
            if (System.IO.File.Exists(Config2.DafaultSettingsFilename))
            {
                SystemSetupViewModel sysVM = new SystemSetupViewModel();
                Assert.That(sysVM.SlotList.Count, Is.GreaterThan(0));
            }
        }
        [Test]
        public void TestCubeSetup1()
        {
            //UeiBridge.Library.CubeSetup cs = new UeiBridge.Library.CubeSetup("pnda://192.168.100.4");
            //UeiBridgeSetup.ViewModels.CubeSetupViewModel cube = new UeiBridgeSetup.ViewModels.CubeSetupViewModel(cs, false);
            //Assert.That(cube.CubeAddress, Is.EqualTo( System.Net.IPAddress.Parse("192.168.100.4") ));

            //Assert.Throws<UeiDaq.UeiDaqException>(() => {  UeiBridge.Library.CubeSetup cs = new UeiBridge.Library.CubeSetup("fff");});
        }
    }
}
