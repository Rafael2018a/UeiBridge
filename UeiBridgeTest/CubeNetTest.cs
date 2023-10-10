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
        public void AddCubeTest()
        {
            Assert.Throws<System.ArgumentException>( () => { ct1.AddCube(null); });

            ct1.AddCube(IPAddress.Parse("1.2.3.4"));
            ct1.AddCube(IPAddress.Parse("4.3.2.1"));
            var p = ct1.PertainCubeList;
            Assert.That(p.Count, Is.EqualTo(3));
        }
        [Test]
        public void StringListCompareTest()
        {
            List<string> l1 = new List<string>(), l2 = new List<string>();
            Assert.That(l1.SequenceEqual(l2), Is.EqualTo(true));
        }

    }
}
