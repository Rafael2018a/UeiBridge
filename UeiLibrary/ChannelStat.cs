//using UeiBridge.Library.Types;

namespace UeiBridge.Library
{
    /// <summary>
    /// Serial channel statistics
    /// </summary>
    public class ChannelStat
    {
        public ChannelStat(int channelIndex)
        {
            this.ChannelIndex = channelIndex;
        }
        public long ReadByteCount { get; set; } = 0;
        public long ReadMessageCount { get; set; } = 0;
        public long WrittenByteCount { get; set; } = 0;
        public long WrittenMessageCount { get; set; } = 0;
        public int ChannelIndex { get; private set; }

        public override string ToString()
        {
            string s = $"ReadBytes:{ReadByteCount} ReadMessages:{ReadMessageCount} WrittenBytes:{WrittenByteCount} WrittenMessages:{WrittenMessageCount}";
            return s;
        }
    }
}
