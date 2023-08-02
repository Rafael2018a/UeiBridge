using System;
//using UeiDaq;
using UeiBridge.Library;
using UeiDaq;

namespace UeiBridge
{
#if dont
    public class DigitalWriterAdapter : IWriterAdapter<UInt16[]>
    {
        UeiDaq.DigitalWriter _ueiDigitalWriter;

        public DigitalWriterAdapter(DigitalWriter ueiDigitalWriter)
        {
            _ueiDigitalWriter = ueiDigitalWriter;
        }

        public void WriteSingleScan(ushort[] scan)
        {
            _ueiDigitalWriter.WriteSingleScanUInt16(scan);
        }

        public void Dispose()
        {
            _ueiDigitalWriter.Dispose();
        }
    }
#endif
}

