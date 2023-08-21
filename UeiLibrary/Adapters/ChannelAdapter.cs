using UeiDaq;

namespace UeiBridge.Library
{
    public class ChannelAdapter : IChannel
    {
        Channel _channel;

        public ChannelAdapter(Channel channel)
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

        public SerialPortSpeed GetSpeed()
        {
            SerialPort sp = (SerialPort)_channel;
            return sp.GetSpeed();
        }
    }




}
