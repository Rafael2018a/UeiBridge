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
            _peekToPeekVoltage = Config.Instance.Analog_Out_PeekVoltage * 2;
            _conversionFactor = _peekToPeekVoltage / UInt16.MaxValue;
            _numberOfChannels = Config.Instance.MaxAnalogOutputChannels;
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
		
            throw new NotImplementedException();
            //UInt32[] result = { 1 };
            //return result;
        }
    }
    class DIO403Convert : IConvert
    {
        public string DeviceName => "DIO-403";
        string _lastError=null;
        readonly int _numberOfOutChannels;

        public DIO403Convert()
        {
            _numberOfOutChannels = Config.Instance.MaxDigital403OutputChannels;
        }

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
            if (messagePayload.Length < _numberOfOutChannels)
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

        //readonly double peekVoltage = Config.Instance.Analog_In_PeekVoltage;
        //const double peekToPeekVoltage = peekVoltage * 2.0;
        //const double uInt16range = UInt16.MaxValue;
        //readonly double _conversionFactor;

        public AI201Converter()
        {
            //peekVoltage = Config.Instance.Analog_In_PeekVoltage;
            //_conversionFactor = int16range / peekToPeekVoltage;
        }
        public byte[] DeviceToEth(object dt)
        {
            double[] inputVector = dt as double[];
            System.Diagnostics.Debug.Assert(null != inputVector);
            byte[] resultVector = new byte[inputVector.Length * 2];
            int ch = 0;
            foreach (double val in inputVector)
            {
                double peekVoltage = Config.Instance.Analog_In_PeekVoltage;
                double p2p = peekVoltage * 2.0;

                //double pVal = (Math.Abs(val) < 0.1) ? 0 : val;
                double zVal = val + peekVoltage; // make zero based
                zVal = (zVal >= p2p) ? p2p : zVal; // protect from high voltage
                double normVal = zVal / p2p; // 0 < normVal < 1

                int vInt = Convert.ToInt32(normVal * (double)UInt16.MaxValue) - (Int32)Int16.MaxValue -1;
                Int16 vShort = Convert.ToInt16((vInt));
                
                byte[] twoBytes = BitConverter.GetBytes(vShort);
                twoBytes.CopyTo(resultVector, ch);
                ch += 2;// sizeof(UInt16);
            }
            return resultVector;
        }
        public object EthToDevice(byte[] messagePayload)
        {
            throw new NotImplementedException();
        }
    }

    class SL508Convert : IConvert
    {
        public string DeviceName => "SL-508-892";

        public string LastErrorMessage => throw new NotImplementedException();

        public byte[] DeviceToEth(object dt)
        {
            System.Diagnostics.Debug.Assert(dt.GetType() == typeof(byte[]));
            byte[] result = dt as byte[];
            return result;
        }

        public object EthToDevice(byte[] messagePayload)
        {
            byte[] newpayload = new byte[messagePayload.Length + 2];
            Array.Copy(messagePayload, newpayload, messagePayload.Length);
            newpayload[newpayload.Length - 1] = 10; // lf
            newpayload[newpayload.Length - 2] = 13; // cr
            return newpayload;
        }
    }
    //
}
