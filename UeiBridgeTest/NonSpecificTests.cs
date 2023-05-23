using NUnit.Framework;
using UeiBridge;
using UeiBridge.Library;
using UeiBridge.Types;

namespace UeiBridgeTest
{
    [TestFixture]
    internal class NonSpecificTests
    {
        [Test]
        public void MessangerTest()
        {
            UdpToSlotMessenger msgr = new UdpToSlotMessenger();
            EnqMock mock = new EnqMock();
            msgr.SubscribeConsumer(mock, 10, 21);
            byte[] payload = new byte[] { 19, 29, 30 };
            var etm = EthernetMessage.CreateMessage(25, 21, 10, payload).GetByteArray(MessageWay.downstream);
            msgr.Enqueue(etm);
            System.Threading.Thread.Sleep(20);

            Assert.That(mock.TheMessage, Is.EqualTo(etm));
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
