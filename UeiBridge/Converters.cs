using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace UeiBridge
{
    class AO308Convert : IConvert
    {
        public string DeviceName => "AO-308";
        string _lastError { get; set; }
        string IConvert.LastErrorMessage => _lastError;

        readonly int _numberOfChannels;
        readonly double  _peekToPeekVoltage;
        readonly double _conversionFactor;

        public AO308Convert()
        {
            _peekToPeekVoltage = Config.Instance.Analog_Out_MinMaxVoltage.Item2 - Config.Instance.Analog_Out_MinMaxVoltage.Item1;
            _conversionFactor = _peekToPeekVoltage / UInt16.MaxValue;
            _numberOfChannels = Config.Instance.MaxAnalogInputChannels;
        }

        public byte[] DeviceToEth(object dt)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Convert from UInt16 to double
        /// </summary>
        public object EthToDevice(byte[] messagePayload)
        {
            if ((messagePayload.Length) < _numberOfChannels * sizeof(UInt16))
            {
                _lastError = $"analog-out message too short. {messagePayload.Length} ";
                return null;
            }

            double[] resultVector = new double[_numberOfChannels];
            for (int chNum = 0; chNum < _numberOfChannels; chNum++)
            {
                Int16 ival = BitConverter.ToInt16(messagePayload, 2 * chNum);
                resultVector[chNum] = ival * _conversionFactor;
            }

            return resultVector;
        }
    }
    class DIO430Convert : IConvert
    {
        public string DeviceName => "DIO-430";
        string _lastError { get; set; }
        string IConvert.LastErrorMessage => _lastError;

        public byte[] DeviceToEth(object dt)
        {
            throw new NotImplementedException();
        }

        public object EthToDevice(byte[] messagePayload)
        {
		// tbd
            throw new NotImplementedException();
            UInt32[] result = { 1 };
            return result;
        }
    }
    class DIO403Convert : IConvert
    {
        public string DeviceName => "DIO-403";
        string _lastError=null;
        string IConvert.LastErrorMessage => _lastError;

        public byte[] DeviceToEth(object dt)
        {
            // int16 vector goes to int8 vector
            // ================================
            UInt16[] deviceVector = (UInt16[])dt;
            byte[] resultVector = new byte[deviceVector.Length];
            Array.Clear(resultVector, 0, resultVector.Length);
            for( int ch = 0; ch< deviceVector.Length; ch++)
            {
                resultVector[ch] = (byte)(deviceVector[ch] & 0xFF);
            }
            return resultVector;
        }
        public object EthToDevice(byte[] messagePayload)
        {
            const int numberOfChannels = 3; 
            if (messagePayload.Length < numberOfChannels)
            {
                _lastError = $"digital-out message too short. {messagePayload.Length} ";
                return null;
            }
            // int8 vector goes to int16 vector
            // ================================
            UInt16[] resultVector = new UInt16[messagePayload.Length];
            Array.Clear(resultVector, 0, resultVector.Length);
            for(int ch=0; ch<messagePayload.Length; ch++)
            {
                resultVector[ch] = messagePayload[ch];
            }
            return resultVector;
        }
    }

    /// <summary>
    /// Convert from double to UIint16
    /// </summary>
    class AI201Converter : IConvert
    {
        public string DeviceName => "AI-201-100";
        string _lastError=null;
        string IConvert.LastErrorMessage => _lastError;

        const double peekVoltage = 12.0;
        const double peekToPeekVoltage = peekVoltage * 2.0;
        const double int16range = UInt16.MaxValue;
        //readonly double _conversionFactor;

        public AI201Converter()
        {
            //_conversionFactor = int16range / peekToPeekVoltage;
        }
        public byte[] DeviceToEth(object dt)
        {
            double[] inputVector = dt as double[];
            System.Diagnostics.Debug.Assert(null != inputVector);
            byte[] resultVector = new byte[inputVector.Length * 2];
            int ch = 0;
            int maxval = Int16.MaxValue;
            //foreach (double val in inputVector)
            //{
            //    double nVal = val + peekVoltage;
            //    nVal = (nVal > peekToPeekVoltage) ? peekToPeekVoltage : nVal;
            //    double normVol = nVal / peekToPeekVoltage;

            //    UInt16 vShort1 = Convert.ToUInt16(normVol * int16range);
            //    UInt16 vShort2 = (UInt16)(vShort1 - maxval);
            //    byte[] two = BitConverter.GetBytes(vShort2);
            //    two.CopyTo(resultVector, ch);
            //    ch += 2;// sizeof(UInt16);
            //}
            foreach (double val in inputVector)
            {
			    // tbd: optimize this
                // tbd. protecet from high voltage.
			
                double pVal = (Math.Abs(val) < 0.1) ? 0 : val;
                double nVal = pVal + peekVoltage;
                nVal = (nVal > peekToPeekVoltage) ? peekToPeekVoltage : nVal;
                double normVol = nVal / peekToPeekVoltage;

                int vShort1 = Convert.ToInt32(normVol * int16range);
                int vShort2 = (vShort1 - maxval);
                
                Int16 vShort3 = Convert.ToInt16((vShort2));
                
                byte[] two = BitConverter.GetBytes(vShort3);
                two.CopyTo(resultVector, ch);
                ch += 2;// sizeof(UInt16);
            }
            return resultVector;
        }
        public object EthToDevice(byte[] messagePayload)
        {
            throw new NotImplementedException();
        }
    }
}
