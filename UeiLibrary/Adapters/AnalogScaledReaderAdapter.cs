using UeiDaq;

namespace UeiBridge.Library
{
    public class AnalogScaledReaderAdapter : IReaderAdapter<double[]>
    {
        AnalogScaledReader _ueiAnalogReader;

        public AnalogScaledReaderAdapter(AnalogScaledReader ueiAnalogReader)
        {
            _ueiAnalogReader = ueiAnalogReader;
        }

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