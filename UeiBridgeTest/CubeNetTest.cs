using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UeiBridge;
using NUnit.Framework;
using System.IO;
using UeiBridge.Library;
using UeiDaq;
using System.Net;
using CubeNet;
using UeiBridge.CubeNet;


namespace UeiBridgeTest
{
    [TestFixture]
    class CubeNetTest
    {
        CubeType ct1;
        CubeType ct1_same;
        CubeType ct2;

        [SetUp]
        public void Setup()
        {
            ct1 = new CubeType("n", "d", "s");
            ct1.AddCube(IPAddress.Parse("10.11.12.13"));
            ct1_same = new CubeType("n", "d", "s");
            ct1_same.AddCube(IPAddress.Parse("10.11.12.13"));

            ct2 = new CubeType("n1", "d", "s");
        }

        [Test]
        [TestCase("", "", null)]
        [TestCase(null,"", null)]
        public void CubeTypeTest(string nickName, string desc, string signature)
        {
            Assert.Throws<System.ArgumentException>( () => { CubeType ct = new CubeType(nickName, desc, signature); });
        }
        [Test]
        public void CubeTypeCompareTest()
        {
            Assert.That(ct1.Equals(null), Is.EqualTo(false));
            Assert.That(ct1.Equals(ct1), Is.EqualTo(true));
            var b = ct1.Equals(ct1_same);
            Assert.That(b, Is.EqualTo(true));
            Assert.That(ct2.Equals(ct1), Is.EqualTo(false));
        }
        [Test]
        public void AddCubeToRepoTest()
        {
            Assert.Throws<System.ArgumentException>( () => { ct1.AddCube(null); });

            ct1.AddCube(IPAddress.Parse("1.2.3.4"));
            ct1.AddCube(IPAddress.Parse("4.3.2.1"));
            var p = ct1.PertainCubeList;
            Assert.That(p.Count, Is.EqualTo(3));
        }

        [Test]
        public void SaveAndLoadRepositoryTest()
        {
            CubeRepositoryProxy crp = new CubeRepositoryProxy();
            crp.CubeRepositoryMain.CubeTypeList.Add(new CubeType("nick55", "desc55", "sig55"));
            crp.CubeRepositoryMain.CubeTypeList.Add(ct1);
            crp.CubeRepositoryMain.CubeTypeList.Add(ct2);

            FileInfo fi = new FileInfo("repoTest.json");
            crp.SaveRepository(fi);

            FileStream fs = fi.OpenRead();
            CubeRepositoryProxy crp1 = new CubeRepositoryProxy(fs);
            fs.Close();
            Assert.That(crp1.CubeRepositoryMain.CubeTypeList, Is.EqualTo(crp.CubeRepositoryMain.CubeTypeList));
            
        }

        [Test]
        public void ViewModelTest()
        {
            MainViewModel mvm = new MainViewModel(null);
            mvm.CreateEmptyRepository(null);// Command.Execute(null);
            mvm.CubeNickname = "nicktest";
            mvm.CubeDesc = "desctest";
            mvm.CubeSignature = "AO-308/DIO-430/";
            mvm.CubeAddress = IPAddress.Parse("192.168.100.2");
            mvm.AddCubeToRepository(null);
            mvm.CreateEmptyRepository(null); // just make sure the command is ignored
            mvm.SaveRepository(null);
            Assert.That(mvm.RepositoryProxy.CubeRepositoryMain.CubeTypeList.Count, Is.EqualTo(1));

            MainViewModel mvm1 = new MainViewModel(null);
            Assert.That(mvm1.RepositoryProxy.CubeRepositoryMain.CubeTypeList.Count, Is.EqualTo(1));
            mvm1.GetFreeIp(null);
            Assert.That(mvm1.CubeAddress, Is.EqualTo(IPAddress.Parse("192.168.100.3")));

        }
    }
}
