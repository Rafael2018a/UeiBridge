using UeiBridge.Library.Interfaces;
using UeiDaq;

namespace UeiBridge.Library
{
    public class ChannelAdapter__old //: IChannel
    {
        Channel _channel;

        public ChannelAdapter__old(Channel channel)
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
