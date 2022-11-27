using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ByteStreamer.Utilities
{
    class MovingAverage
    {

        const int default_filter_length = 5;
        int _filterLength;
        List<int> dataVector;
        int _sum;
        int _average;
        public int Average { get => _average; }
        int _index;
        bool is_filter_complete;

        public MovingAverage(int filterLength)
        {
            _filterLength = filterLength;
            dataVector = new List<int>();
            is_filter_complete = false;
            _index = -1;
            _sum = 0;
            _average = 0;
            for (int i = 0; i < _filterLength; i++)
                dataVector.Add(0);
        }

        public void AddItem(int newItem)
        {
            _index = (_index + 1) % _filterLength;
            _sum -= dataVector[_index];
            dataVector[_index] = newItem;
            _sum += newItem;
            if (!is_filter_complete && _index == _filterLength - 1)
            {
                is_filter_complete = true;
            }
            if (is_filter_complete)
            {
                _average = _sum / _filterLength;
            }
            else
            {
                _average = _sum / (_index + 1);
            }
        }
    };
	
	
    class MovingAverageFloat
    {

        const int default_filter_length = 5;
        int _filterLength;
        List<double> dataVector;
        double _sum;
        double _average;
        public float Average { get => (float) _average; }
        int _index;
        bool is_filter_complete;

        public MovingAverageFloat(int filterLength)
        {
            _filterLength = filterLength;
            dataVector = new List<double>();
            is_filter_complete = false;
            _index = -1;
            _sum = 0;
            _average = 0;
            for (int i = 0; i < _filterLength; i++)
                dataVector.Add(0);
        }

        public void AddItem(double newItem)
        {
            _index = (_index + 1) % _filterLength;
            _sum -= dataVector[_index];
            dataVector[_index] = newItem;
            _sum += newItem;
            if (!is_filter_complete && _index == _filterLength - 1)
            {
                is_filter_complete = true;
            }
            if (is_filter_complete)
            {
                _average = _sum / _filterLength;
            }
            else
            {
                _average = _sum / (_index + 1);
            }
        }
    };

}


