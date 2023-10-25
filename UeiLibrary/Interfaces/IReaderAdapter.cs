using System;
using UeiDaq;

namespace UeiBridge.Interfaces
{
    public interface IReaderAdapter<T> : IDisposable
    {
        T LastScan { get; }
        T ReadSingleScan();
    }

    public interface ICANReaderAdapter : IReaderAdapter<CANFrame>
    {
        IAsyncResult BeginRead(int numberOfFrames, AsyncCallback readerCallback, int ch);

        CANFrame[] EndRead(IAsyncResult ar);

    }


}
