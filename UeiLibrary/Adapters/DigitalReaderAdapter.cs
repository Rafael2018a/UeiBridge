using System;
using UeiBridge.Library.Interfaces;
using UeiDaq;

namespace UeiBridge.Library
{
    public class DigitalReaderAdapter : IReaderAdapter<UInt16[]>
    {
        private DigitalReader _digitalReader;
        public UInt16[] LastScan { get; set; }

        public DigitalReaderAdapter(DigitalReader digitalReader)
        {
            this._digitalReader = digitalReader;
        }
        public UInt16[] ReadSingleScan()
        {
            LastScan = _digitalReader.ReadSingleScanUInt16();
            return LastScan;
        }
        public void Dispose()
        {
            this._digitalReader.Dispose();
        }
    }
}
