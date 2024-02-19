using System;
using UeiDaq;
using UeiBridge.Library.Interfaces;

namespace UeiBridge.Library
{
    public class AnalogScaledWriteAdapter : IWriterAdapter<double[]>
    {
        AnalogScaledWriter _ueiAnalogWriter;
        double[] _lastScan; 
        //Session _originSession;

        public AnalogScaledWriteAdapter(AnalogScaledWriter analogWriter)
        {
            this._ueiAnalogWriter = analogWriter;
            //_originSession = originSession;
        }

        public double[] LastScan => _lastScan;

        //public Session OriginSession => _originSession;

        public void Dispose()
        {
            
        }

        public void WriteSingleScan(double[] scan)
        {
            _ueiAnalogWriter.WriteSingleScan(scan);
            _lastScan = scan;
        }
    }
}