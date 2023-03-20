using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UeiDaq;

namespace UeiBridge.Library
{
    interface ProjectInterfaces
    {

    }
    public interface IWriterAdapter<T>
    {
        void WriteSingleScan(T scan);
        //int NumberOfChannels { get; }
        UeiDaq.Session OriginSession { get; }
    }
    //public interface IDigitalWriter
    //{
    //    bool WriteScan(UInt16[] scan);
    //}
}
