using UeiBridge.Interfaces;
using UeiDaq;

namespace UeiBridge.Library
{
    /// <summary>
    /// This class wraps uei-analog-reader
    /// </summary>
    public class AnalogScaledReaderAdapter : IReaderAdapter<double[]>
    {
        AnalogScaledReader _ueiAnalogReader;

        public AnalogScaledReaderAdapter(AnalogScaledReader ueiAnalogReader)
        {
            _ueiAnalogReader = ueiAnalogReader;
        }

        public double[] LastScan => throw new System.NotImplementedException();

        public void Dispose()
        {
            _ueiAnalogReader?.Dispose();
        }

        public double[] ReadSingleScan()
        {
            return _ueiAnalogReader.ReadSingleScan();
        }
    }
}