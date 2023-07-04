using System;
using UeiDaq;

namespace UeiBridge.Library
{
    public class DigitalWriterAdapter : IWriterAdapter<UInt16[]>
    {
        private DigitalWriter _digitalWriter;

        public DigitalWriterAdapter(DigitalWriter digitalWriter)
        {
            _digitalWriter = digitalWriter;
        }

        public void Dispose()
        {
            _digitalWriter.Dispose();
        }

        public void WriteSingleScan(ushort[] scan)
        {
            _digitalWriter.WriteSingleScanUInt16(scan);
        }
    }




}
