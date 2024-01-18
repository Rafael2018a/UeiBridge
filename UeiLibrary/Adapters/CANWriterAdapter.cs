using System;
using UeiBridge.Library.Interfaces;
using UeiDaq;

namespace UeiBridge.Library
{
    public class CANWriterAdapter : IWriterAdapter<CANFrame>
    {
        CANWriter _canWriter;
        public CANFrame LastScan => throw new NotImplementedException();

        public CANWriterAdapter(CANWriter canWriter)
        {
            _canWriter = canWriter;
        }

        public void Dispose()
        {
            _canWriter?.Dispose();
        }

        public void WriteSingleScan(CANFrame scan)
        {
            throw new NotImplementedException();
        }

        public void Write(CANFrame[] frames)
        {
            _canWriter.Write(frames);
        }
    }


}
