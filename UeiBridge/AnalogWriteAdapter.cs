using System;
using UeiDaq;
using UeiBridge.Library;

namespace UeiBridge
{
#if dont
    public class AnalogWriteAdapter : IWriterAdapter<double[]>
    {
        AnalogScaledWriter _ueiAnalogWriter;
        Session _originSession;

        public AnalogWriteAdapter(AnalogScaledWriter analogWriter, Session originSession)
        {
            this._ueiAnalogWriter = analogWriter;
            _originSession = originSession;
        }

        public Session OriginSession => _originSession;

        public void Dispose()
        {
            
        }

        public void WriteSingleScan(double[] scan)
        {
            _ueiAnalogWriter.WriteSingleScan(scan);
        }
    }
#endif
}