using System;
using UeiDaq;

namespace UeiBridge.Library
{
    public class SerialReaderAdapter : IReaderAdapter<byte[]>
    {
        private SerialReader sl;

        public SerialReaderAdapter(SerialReader sl)
        {
            this.sl = sl;
        }

        public byte[] LastScan => throw new NotImplementedException();

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public byte[] ReadSingleScan()
        {
            throw new NotImplementedException();
        }

        public IAsyncResult BeginRead(int minLen, AsyncCallback readerAsyncCallback, int ch1)
        {
            return sl.BeginRead(minLen, readerAsyncCallback, ch1);
        }

        public byte[] EndRead(IAsyncResult ar)
        {
            return sl.EndRead(ar);
        }
    }


}
