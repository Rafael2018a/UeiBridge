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
}
