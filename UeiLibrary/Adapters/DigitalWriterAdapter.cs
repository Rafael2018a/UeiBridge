using System;
using UeiDaq;

namespace UeiBridge.Library
{
    public class DigitalWriterAdapter : IWriterAdapter<UInt16[]>
    {
        DigitalWriter _digitalWriter;
        UInt16[] _lastScan;

        public DigitalWriterAdapter(DigitalWriter digitalWriter)
        {
            _digitalWriter = digitalWriter;
        }

        public ushort[] LastScan => _lastScan;

        public void Dispose()
        {
            _digitalWriter.Dispose();
        }

        public void WriteSingleScan(ushort[] scan)
        {
            _digitalWriter.WriteSingleScanUInt16(scan);
            _lastScan = scan;
        }
    }


}
