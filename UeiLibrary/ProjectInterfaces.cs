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
    public interface IAnalogWrite
    {
        void WriteSingleScan(double[] scen);
        int NumberOfChannels { get; set; }
    }
    public interface DigitalWrite
    {
        bool WriteScan(UInt16[] scan);
    }
}
