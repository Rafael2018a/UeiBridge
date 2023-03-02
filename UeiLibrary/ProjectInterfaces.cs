using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UeiBridge.Library
{
    interface ProjectInterfaces
    {

    }
    public interface IAnalogWriter
    {
        void WriteSingleScan(double[] scen);
        int NumberOfChannels { get; }
    }
    public interface DigitalWrite
    {
        bool WriteScan(UInt16[] scan);
    }
}
