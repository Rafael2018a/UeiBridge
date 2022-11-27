using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ByteStreamer.Utilities
{
    class DelayVector
    {
        byte[] _delayVector;
        List<int> _filledCells = new List<int>();
        List<int> _emptyCells = new List<int>();
        Random _random = new Random();

        public int Length { get => _delayVector.Length; }

        public DelayVector(int vectorLength)
        {
            _delayVector = new byte[vectorLength];
            // prolog
            _filledCells.RemoveRange(0, _filledCells.Count);
            _emptyCells.RemoveRange(0, _emptyCells.Count);

            for (int i = 0; i < _delayVector.Length; i++)
            {
                _delayVector[i] = 0;
                _emptyCells.Add(i);
            }
        }
        public void SetFillPercent(double desiredPercent)
        {
            Int32 desiredNumberOfCells = Convert.ToInt32(desiredPercent * _delayVector.Length / 100.0);
            int numberOfIterations = 0;

            if (desiredNumberOfCells !=_filledCells.Count)
            {
                System.Diagnostics.Trace.WriteLine("ByteStreamer: Adjusting rate");
            }

            while (desiredNumberOfCells > _filledCells.Count)
            {
                numberOfIterations++;
                int r = _random.Next(_emptyCells.Count);
                int cellNumber = _emptyCells[r];
                _emptyCells.RemoveAt(r);
                _filledCells.Add(cellNumber);
                System.Diagnostics.Debug.Assert(0 == _delayVector[cellNumber]);
                _delayVector[cellNumber] = 1;
            }

            // reduce
            while (desiredNumberOfCells < _filledCells.Count)
            {
                numberOfIterations++;
                int r = _random.Next(_filledCells.Count);
                int cellNumber = _filledCells[r];
                _filledCells.RemoveAt(r);
                _emptyCells.Add(cellNumber);
                System.Diagnostics.Debug.Assert((1 == _delayVector[cellNumber]));
                _delayVector[cellNumber] = 0;
            }

            System.Diagnostics.Debug.Assert((_emptyCells.Count + _filledCells.Count) == _delayVector.Length);

            //return numberOfIterations;

        }


        public byte this[int index]
        {
            get => _delayVector[index];
        }

    }
}
