using System;

namespace UeiBridge.Library
{
    public interface IReaderAdapter<T> : IDisposable
    {
        T LastScan { get; }
        T ReadSingleScan();
    }




}
