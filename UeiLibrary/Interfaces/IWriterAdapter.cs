using System;

namespace UeiBridge.Library
{
    public interface IWriterAdapter<T> : IDisposable
    {
        void WriteSingleScan(T scan);
    }




}
