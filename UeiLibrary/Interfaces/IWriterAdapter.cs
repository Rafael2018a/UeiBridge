using System;

namespace UeiBridge.Library
{
    public interface IWriterAdapter<T> : IDisposable
    {
        T LastScan { get; }
        void WriteSingleScan(T scan);
    }

}
