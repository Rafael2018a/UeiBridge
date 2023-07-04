using System;
using UeiDaq;
using UeiBridge.Library;

namespace UeiBridge.Library
{
    public class AnalogScaledWriteAdapter : IWriterAdapter<double[]>
    {
        AnalogScaledWriter _ueiAnalogWriter;
        //Session _originSession;

        public AnalogScaledWriteAdapter(AnalogScaledWriter analogWriter)
        {
            this._ueiAnalogWriter = analogWriter;
            //_originSession = originSession;
        }

        //public Session OriginSession => _originSession;

        public void Dispose()
        {
            
        }

        public void WriteSingleScan(double[] scan)
        {
            _ueiAnalogWriter.WriteSingleScan(scan);
        }
    }
}