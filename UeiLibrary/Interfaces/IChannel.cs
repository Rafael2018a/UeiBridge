using UeiDaq;

namespace UeiBridge.Library
{
    public interface IChannel
    {
        string GetResourceName(); // for example: "pdna://192.168.100.2/Dev5/Di0,2,4"
        int GetIndex();
        SerialPortSpeed GetSpeed();
    }




}
