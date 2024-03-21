using System;
using System.Net;

namespace UeiBridge.Library
{
    /// <summary>
    /// This class encapsulates payload with dest address
    /// </summary>
    public class SendObject
    {
        public IPEndPoint TargetEndPoint { get; }
        public byte[] ByteMessage { get; }
        public SendObject(IPEndPoint targetEndPoint, byte[] byteMessage)
        {
            TargetEndPoint = targetEndPoint;
            ByteMessage = byteMessage;
        }
    }
    public class SendObject2
    {
        public IPEndPoint TargetEndPoint { get; }
        public byte[] RawByteMessage { get; }
        public Func<byte[], byte[]> MessageBuilder { get; }
        public SendObject2(IPEndPoint targetEndPoint, Func<byte[], byte[]> builder, byte[] rawByteMessage)
        {
            this.TargetEndPoint = targetEndPoint;
            this.RawByteMessage = rawByteMessage;
            this.MessageBuilder = builder;
        }
    }
}
