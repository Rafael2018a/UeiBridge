using UeiDaq;

namespace UeiBridge.Interfaces
{
    public interface IDevice
    {
        Range[] GetAORanges();
        Range[] GetAIRanges();
    }
}
