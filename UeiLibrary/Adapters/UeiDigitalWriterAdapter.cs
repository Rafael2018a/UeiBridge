using System;
using UeiDaq;

namespace UeiBridge.Library
{
    public class UeiDigitalWriterAdapter : IWriterAdapter<UInt16[]>
    {
        private DigitalWriter _digitalWriter;

        public UeiDigitalWriterAdapter(DigitalWriter digitalWriter)
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
