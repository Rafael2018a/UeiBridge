using System;
using UeiDaq;

namespace UeiBridge.Library
{
    public class CANReaderAdapter : IReaderAdapter<CANFrame>
    {
        private CANReader cANReader;

        public CANReaderAdapter(CANReader cANReader)
        {
            this.cANReader = cANReader;
        }

        public CANFrame LastScan => throw new NotImplementedException();

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public CANFrame ReadSingleScan()
        {
            throw new NotImplementedException();
        }
    }


}
