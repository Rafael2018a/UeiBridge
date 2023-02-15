using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using UeiBridge.Types;

namespace UeiBridge
{
    class AO308Convert : IConvert
    {
        public string DeviceName => "AO-308";
        string _lastError { get; set; }
        string IConvert.LastErrorMessage => _lastError;

        const int _numberOfChannels = 8;
        readonly double  _peekToPeekVoltage;
        readonly double _conversionFactor;

        public AO308Convert(DeviceSetup setup)
        {
            AO308Setup ao308 = setup as AO308Setup;
            if (null == ao308)
                return;
            
            _peekToPeekVoltage = ao308.PeekVoltage_Out * 2;  //Analog_Out_PeekVoltage * 2;
            _conversionFactor = _peekToPeekVoltage / UInt16.MaxValue;
            //_numberOfChannels = Config.Instance.MaxAnalogOutputChannels;
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
#if Obsolete
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
#endif
    class DIO403Convert : IConvert
    {
        public string DeviceName => "DIO-403";
        string _lastError = null;
        readonly int _numberOfOutChannels;

        public DIO403Convert(DeviceSetup setup)
        {
            _numberOfOutChannels = 3;// Config.Instance.MaxDigital403OutputChannels;
        }

        string IConvert.LastErrorMessage => _lastError;

        public byte[] DeviceToEth(object dt)
        {
            // int16 vector goes to int8 vector
            // ================================
            UInt16[] deviceVector = (UInt16[])dt;
            byte[] resultVector = new byte[deviceVector.Length];
            Array.Clear(resultVector, 0, resultVector.Length);
            for (int ch = 0; ch < deviceVector.Length; ch++)
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
            for (int ch = 0; ch < messagePayload.Length; ch++)
            {
                resultVector[ch] = messagePayload[ch];
            }
            return resultVector;
        }
    }
    class DIO470Convert : IConvert
    {
        public string DeviceName => "DIO-470";
        string _lastError = null;
        readonly int _numberOfOutChannels;

        public DIO470Convert(DeviceSetup setup)
        {
            _numberOfOutChannels = 3;// Config.Instance.MaxDigital403OutputChannels;
        }

        string IConvert.LastErrorMessage => _lastError;

        public byte[] DeviceToEth(object dt)
        {
            // int16 vector goes to int8 vector
            // ================================
            UInt16[] deviceVector = (UInt16[])dt;
            byte[] resultVector = new byte[deviceVector.Length];
            Array.Clear(resultVector, 0, resultVector.Length);
            for (int ch = 0; ch < deviceVector.Length; ch++)
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
            for (int ch = 0; ch < messagePayload.Length; ch++)
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
        //double _peekVoltage;
        AI201100Setup _thisDeviceSsetup;

        public AI201Converter( DeviceSetup setup)
        {
            _thisDeviceSsetup = setup as AI201100Setup;
            //AI201100Setup thissetup = setup as AI201100Setup;
            //if (null != thissetup)
            {
                //_peekVoltage = thissetup.PeekVoltage;
            }
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
                double clippedVal = (val > 12.0) ? 12.0 : val;
                clippedVal = (clippedVal < -12.0) ? -12.0 : clippedVal;
                double p2p = _thisDeviceSsetup.PeekVoltage_In * 2.0;

                double zVal = clippedVal + _thisDeviceSsetup.PeekVoltage_In; // make zero based
                System.Diagnostics.Debug.Assert(zVal >= 0.0);
                //zVal = (zVal >= p2p) ? p2p : zVal; // protect from high voltage
                double normVal = zVal / p2p; 

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
        public SL508Convert(DeviceSetup setup)
        {
        }

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
            return messagePayload;
            //byte[] newpayload = new byte[messagePayload.Length + 2];
            //Array.Copy(messagePayload, newpayload, messagePayload.Length);
            //newpayload[newpayload.Length - 1] = 10; // lf
            //newpayload[newpayload.Length - 2] = 13; // cr
            //return newpayload;
        }
    }
    //
}
