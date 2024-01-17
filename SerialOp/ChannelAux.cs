using System;
using UeiDaq;

namespace SerialOp
{
    /// <summary>
    /// Auxiliary class for serial channel
    /// channel index, channel nickname (tbd)
    /// the originating session, the associated serial reader
    /// and more..
    /// </summary>
    class ChannelAux
    {
        public SerialReader Reader { get; set; }
        public IAsyncResult AsyncResult { get; set; }
        public int ChannelIndex { get; private set; } // zero based
        //public int SelfIndex { get; private set; }
        public Session OriginatingSession { get; private set; }
        public ChannelAux(int channelIndex, Session originatingSession)
        {
            this.ChannelIndex = channelIndex;
            this.OriginatingSession = originatingSession;
        }
    }
}
