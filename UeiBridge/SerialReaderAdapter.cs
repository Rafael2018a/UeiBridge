using System;
using UeiDaq;
using UeiBridge.Library;

namespace UeiBridge
{
    public class SerialReaderAdapter : IReaderAdapter<byte[]>
    {
        private SerialReader _serialReader;

        public SerialReaderAdapter(SerialReader sr)
        {
            this._serialReader = sr;
        }

        public IAsyncResult BeginRead(int numBytes, AsyncCallback readerCallback, int channel)
        {
            return _serialReader.BeginRead(numBytes, readerCallback, channel);
        }

        public void Dispose()
        {
            this._serialReader.Dispose();
        }

        public byte[] EndRead(IAsyncResult ar)
        {
            return this._serialReader.EndRead(ar);
        }
    }
}