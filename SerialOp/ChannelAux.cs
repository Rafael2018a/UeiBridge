using System;
using System.Threading.Tasks;
using UeiDaq;

namespace SerialOp
{
    /// <summary>
    /// Auxiliary class for serial channel
    /// channel index, channel nickname (tbd)
    /// the originating session, the associated serial reader
    /// and more..
    /// </summary>
    public class ChannelAux
    {
        public SerialReader Reader { get; private set; }
        public SerialWriter Writer { get; private set; }
        public IAsyncResult AsyncResult { get; set; }
        public int ChannelIndex { get; private set; } // zero based
        //public int SelfIndex { get; private set; }
        public Session OriginatingSession { get; private set; }
        public ChannelAux(int channelIndex, SerialReader reader, SerialWriter writer,  Session originatingSession)
        {
            this.ChannelIndex = channelIndex;
            this.Reader = reader;
            this.Writer = writer;
            this.OriginatingSession = originatingSession;
        }
        //public Task<ChannelAux> ReadTask { get; private set; }
        public Task ReadTask { get;  set; }
    }
}
