using NUnit.Framework;
using UeiBridge;
using UeiBridge.Interfaces;
using UeiBridge.Library;
using UeiBridge.Types;

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
    }
    class EnqMock : IEnqueue<byte[]>
    {
        public byte[] TheMessage { get; set; }
        public void Enqueue(byte[] i)
        {
            TheMessage = i;
        }
    }
}
