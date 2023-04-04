using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using UeiBridgeSetup;

namespace UeiBridgeTest
{
    [TestFixture]
    class ViewModelTests
    {
        [Test]
        public void TestEmptyEndPointVM1()
        {
            UeiBridgeSetup.ViewModels.EndPointViewModel vm = new UeiBridgeSetup.ViewModels.EndPointViewModel(UeiBridgeSetup.ViewModels.EndPointLocation.Dest);

            Assert.Multiple(() =>
            {
                Assert.That(vm.IpString, Is.EqualTo("0.0.0.0"));
                Assert.That(vm.IpPort, Is.EqualTo(0));
            });
        }
        [Test]
        public void TestEndPointVM2()
        {
            UeiBridgeSetup.ViewModels.EndPointViewModel vm = new UeiBridgeSetup.ViewModels.EndPointViewModel(UeiBridgeSetup.ViewModels.EndPointLocation.Dest, new System.Net.IPEndPoint( System.Net.IPAddress.Parse("8.8.8.8"), 5050));
            
            Assert.Multiple(() =>
            {
                Assert.That(vm.IpString, Is.EqualTo("8.8.8.8"));
                Assert.That(vm.IpPort, Is.EqualTo(5050));
            });
        }

        [Test]
        public void TestCubeSetupVM1()
        {
            UeiBridge.Library.CubeSetup cs = new UeiBridge.Library.CubeSetup("simu://");
            UeiBridgeSetup.ViewModels.CubeSetupViewModel cube = new UeiBridgeSetup.ViewModels.CubeSetupViewModel(cs, false);
            Assert.That(cube.CubeAddress, Is.Null);
        }
        [Test]
        public void TestCubeSetupVM2()
        {
            //UeiBridge.Library.CubeSetup cs = new UeiBridge.Library.CubeSetup("pnda://192.168.100.4");
            //UeiBridgeSetup.ViewModels.CubeSetupViewModel cube = new UeiBridgeSetup.ViewModels.CubeSetupViewModel(cs, false);
            //Assert.That(cube.CubeAddress, Is.EqualTo( System.Net.IPAddress.Parse("192.168.100.4") ));
        }
        [Test]
        public void TestCubeSetup1()
        {
            Assert.Throws<UeiDaq.UeiDaqException>(() => {  UeiBridge.Library.CubeSetup cs = new UeiBridge.Library.CubeSetup("fff");});
        }
    }
}
