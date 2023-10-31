using System;
using UeiBridge.Interfaces;
using UeiDaq;

namespace UeiBridge.Library
{
    public class SerialReaderAdapter : IReaderAdapter<byte[]>
    {
        private SerialReader _serialReader;

        public SerialReaderAdapter(SerialReader sl)
        {
            this._serialReader = sl;
        }

        public byte[] LastScan => throw new NotImplementedException();

        public void Dispose()
        {
            _serialReader.Dispose();
        }

        public byte[] ReadSingleScan()
        {
            throw new NotImplementedException();
        }

        public IAsyncResult BeginRead(int minLen, AsyncCallback readerAsyncCallback, int ch1)
        {
            return _serialReader.BeginRead(minLen, readerAsyncCallback, ch1);
        }

        public byte[] EndRead(IAsyncResult ar)
        {
            return _serialReader.EndRead(ar);
        }
    }


}
