using System;

namespace UeiBridge.Library.Interfaces
{
    public interface IWriterAdapter<T> : IDisposable
    {
        T LastScan { get; }
        void WriteSingleScan(T scan);
    }

}
