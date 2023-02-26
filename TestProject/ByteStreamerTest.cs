using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using ByteStreamer3;

namespace TestProject
{
    [TestFixture]
    public class ByteStreamerTest
    {
        [Test]
        public void TestPlayItem()
        {
            PlayItem pi=null;// = new PlayItem();
            Assert.That(pi, Is.Null);
            //Assert.Pass();
        }
    }
}
