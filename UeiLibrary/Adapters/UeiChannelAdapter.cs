using UeiDaq;

namespace UeiBridge.Library
{
    public class UeiChannelAdapter : IChannel
    {
        Channel _channel;

        public UeiChannelAdapter(Channel channel)
        {
            _channel = channel;
        }

        public int GetIndex()
        {
            return _channel.GetIndex();
        }

        public string GetResourceName()
        {
            return _channel.GetResourceName();
        }
    }




}
