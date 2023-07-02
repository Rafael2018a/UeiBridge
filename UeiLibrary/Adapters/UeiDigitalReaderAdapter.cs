﻿using System;
using UeiDaq;

namespace UeiBridge.Library
{
    public class UeiDigitalReaderAdapter : IReaderAdapter<UInt16[]>
    {
        private DigitalReader _digitalReader;

        public UeiDigitalReaderAdapter(DigitalReader digitalReader)
        {
            this._digitalReader = digitalReader;
        }
        public UInt16[] ReadSingleScan()
        {
            return _digitalReader.ReadSingleScanUInt16();
        }
        public void Dispose()
        {
            this._digitalReader.Dispose();
        }
    }




}
