using System;

namespace UeiBridge.Library
{
    public interface IReaderAdapter<T> : IDisposable
    {
        //UInt16[] 
        T ReadSingleScan();
    }




}
