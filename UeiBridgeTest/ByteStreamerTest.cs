using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
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
            PlayFile pi=null;// = new PlayItem();
            Assert.That(pi, Is.Null);
            //Assert.Pass();
        }

        public class Consumner : UeiBridge.Types.IEnqueue<byte[]>
        {
            public byte[] receivedMessage { get; set; }
            public void Enqueue(byte[] i)
            {
                receivedMessage = i;
            }
        }
        // [Test]
        public void TestMCastClientServer()
        {
            IPAddress localNic = IPAddress.Parse("192.168.1.154");
            IPEndPoint mcEndpoint = new IPEndPoint( IPAddress.Parse("231.168.19.10"), 7094);
            Consumner consumer = new Consumner();
            UeiBridge.UdpReader mcReader = new UeiBridge.UdpReader(mcEndpoint, localNic, consumer, "abcd");
            mcReader.Start();

            ByteStreamer3.UdpWriter writer = new ByteStreamer3.UdpWriter(mcEndpoint);
            string str = "MightAsWell";
            byte[] bytes = Encoding.ASCII.GetBytes(str);
            writer.Send( bytes);
            System.Threading.Thread.Sleep(100);

            string theMessage = Encoding.ASCII.GetString(consumer.receivedMessage);

            Assert.That(str, Is.EqualTo(theMessage));
        }
    }

}
