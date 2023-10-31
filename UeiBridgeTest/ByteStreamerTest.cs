using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using NUnit.Framework;
using ByteStreamer3;
using System.IO;
using Newtonsoft.Json;
using ByteStreamer3.Utilities;
using UeiBridge.Types;
using UeiBridge.Interfaces;

namespace UeiBridgeTest
{
    [TestFixture]
    public class ByteStreamerTest
    {
        [Test]
        public void TestPlayItem()
        {
            JFileAux pi=null;// = new PlayItem();
            Assert.That(pi, Is.Null);
            //Assert.Pass();
        }

        public class Consumer : IEnqueue<SendObject>
        {
            public byte[] receivedMessage { get; set; }
            public void Enqueue(SendObject so)
            {
                receivedMessage = so.ByteMessage;
            }
        }
        [Test]
        public void TestMCastClientServer()
        {
            IPAddress localNic = null;// IPAddress.Parse("192.168.1.154");
            IPEndPoint mcEndpoint = new IPEndPoint( IPAddress.Parse("231.168.19.10"), 7094);
            Consumer consumer = new Consumer();
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
        [Test]
        public void DeserializeObjectTest()
        {

            FileInfo _playFile = new FileInfo(@"..\..\..\..\ByteStreamer3\SampleJson\sample for unit test.json");

            using (StreamReader reader = _playFile.OpenText())
            {
                JFileClass jFileObject = JsonConvert.DeserializeObject<JFileClass>(reader.ReadToEnd());
                Assert.Multiple(() =>
                { 
                    Assert.That(jFileObject, Is.Not.Null);
                    Assert.That(jFileObject.Header, Is.Not.Null);
                    Assert.That(jFileObject.Header.ConverterName, Is.Not.Null);
                });
                    //
            }

            
        }
    }

}
