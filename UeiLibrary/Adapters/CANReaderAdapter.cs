using System;
using UeiBridge.Interfaces;
using UeiDaq;

namespace UeiBridge.Library
{
    public class CANReaderAdapter : ICANReaderAdapter
    {
        private CANReader _canReader;

        public CANReaderAdapter(CANReader cANReader)
        {
            this._canReader = cANReader;
        }

        public CANFrame LastScan => throw new NotImplementedException();

        public void Dispose()
        {
            this._canReader.Dispose();
        }

        public CANFrame ReadSingleScan()
        {
            throw new NotImplementedException();
        }

        public IAsyncResult BeginRead(int numberOfFrames, AsyncCallback readerCallback, int ch)
        {
            return _canReader.BeginRead(numberOfFrames, readerCallback, ch);
        }

        public CANFrame[] EndRead(IAsyncResult ar)
        {
            return _canReader.EndRead(ar);
        }
    }


}
