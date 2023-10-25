using System;

namespace UeiBridge.Interfaces
{
    public interface IWriterAdapter<T> : IDisposable
    {
        T LastScan { get; }
        void WriteSingleScan(T scan);
    }

}
