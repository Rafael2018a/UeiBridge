using NUnit.Framework;
using System;
using UeiBridge;
using UeiBridge.Library.Interfaces;
using UeiBridge.Library;
using UeiBridge.Library.Types;

namespace UeiBridgeTest
{
    [TestFixture]
    internal class NonSpecificTests
    {
        [Test]
        public void MessengerTest()
        {
            UdpToSlotMessenger msgr = new UdpToSlotMessenger();
            EnqMock mock = new EnqMock();
            msgr.SubscribeConsumer(mock, 10, 21);
            byte[] payload = new byte[] { 19, 29, 30 };
            var etm = EthernetMessage.CreateMessage(25, 21, 10, payload).GetByteArray(MessageWay.downstream);
            msgr.Enqueue(new SendObject(null, etm));
            System.Threading.Thread.Sleep(20);

            Assert.That(mock.TheMessage, Is.EqualTo(etm));
        }
        [Test]
        public void EndPointTest()
        {
            EndPoint ep1 = EndPoint.MakeEndPoint("nonvalid", 25);
            EndPoint ep2 = EndPoint.MakeEndPoint("8.8.8.8", 0);
            Assert.That(ep1, Is.Null);
            Assert.That(ep2, Is.Not.Null);
        }
        [Test]
        public void UriClassTest()
        {
            {
                string uri1 = "pdna://192.168.100.2";
                Uri resutlUri;
                bool ok1 = Uri.TryCreate(uri1, UriKind.Absolute, out resutlUri);
            }
            {
                string uri1 = "192.168.100.2";
                Uri resutlUri;
                bool ok1 = Uri.TryCreate(uri1, UriKind.RelativeOrAbsolute, out resutlUri);
            }
            {
                string uri1 = "pdna://192.168.100.2/Dev0";
                Uri resutlUri;
                bool ok1 = Uri.TryCreate(uri1, UriKind.RelativeOrAbsolute, out resutlUri);
            }
            {
                string uri1 = "simu://Dev0";
                Uri resutlUri;
                bool ok1 = Uri.TryCreate(uri1, UriKind.RelativeOrAbsolute, out resutlUri);
            }
            {
                string uri1 = "simu://";
                Uri resutlUri;
                bool ok1 = Uri.TryCreate(uri1, UriKind.RelativeOrAbsolute, out resutlUri);
            }

        }
        [Test]
        public void CubePingTest()
        {
            CubeOp.Program p = new CubeOp.Program();
            string[] cmdline = { "192.168.100.3", "--cube-ping" };
            p.Run(cmdline);
        }
    }
    class EnqMock : IEnqueue<byte[]>
    {
        public byte[] TheMessage { get; set; }

        public void Dispose()
        {
            
        }

        public void Enqueue(byte[] i)
        {
            TheMessage = i;
        }
    }
}
