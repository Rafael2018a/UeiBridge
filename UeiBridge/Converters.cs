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

        const int numberOfChannels = 8; // from icd
        readonly double  _peekToPeekVoltage;
        readonly double _conversionFactor;


        public AO308Convert()
        {
            _peekToPeekVoltage = Config.Instance.AnalogOutMinMaxVoltage.Item2 - Config.Instance.AnalogOutMinMaxVoltage.Item1;
            _conversionFactor = _peekToPeekVoltage / (Int16.MaxValue - Int16.MinValue);
        }

        public byte[] DeviceToEth(object dt)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Convert from UInt16 to double
        /// </summary>
        /// <param name="messagePayload"></param>
        /// <returns></returns>
        public object EthToDevice(byte[] messagePayload)
        {

            if ((messagePayload.Length) < numberOfChannels * sizeof(UInt16))
            {
                _lastError = $"analog-out message too short. {messagePayload.Length} ";
                return null;
            }
            
            double[] resultVector = new double[numberOfChannels];
            for (int chNum = 0; chNum < numberOfChannels; chNum++)
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
            throw new NotImplementedException();
            //UInt32[] result = { 1 };
            //return result;
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

        const double peekToPeekVoltage = 24.0;
        readonly double _conversionFactor;

        public AI201Converter()
        {
            _conversionFactor = peekToPeekVoltage / (Int16.MaxValue - Int16.MinValue);
        }
        public byte[] DeviceToEth(object dt)
        {
            double[] inputVector = dt as double[];
            System.Diagnostics.Debug.Assert(null != inputVector);
            byte[] resultVector = new byte[inputVector.Length * 2];
            int ch = 0;
            foreach (double val in inputVector)
            {
                UInt16 r = Convert.ToUInt16( val / _conversionFactor);
                BitConverter.GetBytes(r).CopyTo(resultVector, ch);
                ch += sizeof(UInt16);
            }
            return resultVector;
        }
        public object EthToDevice(byte[] messagePayload)
        {
            throw new NotImplementedException();
        }
    }
}
