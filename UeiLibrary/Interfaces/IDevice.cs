using UeiDaq;

namespace UeiBridge.Library
{
    public interface IDevice
    {
        Range[] GetAORanges();
        Range[] GetAIRanges();
    }
}
