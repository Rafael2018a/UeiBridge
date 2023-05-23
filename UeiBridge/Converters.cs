using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using UeiBridge.Types;
using UeiBridge.Library;

namespace UeiBridge
{
#if dont
    [Obsolete]
    class DIO403Convert : IConvert
    {
        public string DeviceName => "DIO-403";
        string _lastError = null;
        readonly int _numberOfOutChannels;

        [Obsolete]
        public DIO403Convert(DeviceSetup setup)
        {
            _numberOfOutChannels = 3;// Config.Instance.MaxDigital403OutputChannels;
        }
        public DIO403Convert( int numberOfChannels)
        {
            _numberOfOutChannels = numberOfChannels;
        }

        string IConvert.LastErrorMessage => _lastError;

        public byte[] DeviceToEth(object dt)
        {
            // convert int16 vector to int8 vector
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
            // convert int8 vector to int16 vector
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
#endif
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
#if remove
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
                double p2p = AI201100Setup.PeekVoltage_upstream * 2.0;

                double zVal = clippedVal + AI201100Setup.PeekVoltage_upstream; // make zero based
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
#endif
    public class DigitalConverter : IConvert2<UInt16[]>
    {
        public UInt16[] DownstreamConvert(byte[] messagePayload)
        {
            // convert int8 vector to int16 vector
            // ================================
            UInt16[] resultVector = new UInt16[messagePayload.Length];
            Array.Clear(resultVector, 0, resultVector.Length);
            for (int ch = 0; ch < messagePayload.Length; ch++)
            {
                resultVector[ch] = messagePayload[ch];
            }
            return resultVector;
        }

        public byte[] UpstreamConvert(UInt16[] dt)
        {
            // convert int16 vector to int8 vector
            // ================================
            UInt16[] deviceVector = dt;
            byte[] resultVector = new byte[deviceVector.Length];
            Array.Clear(resultVector, 0, resultVector.Length);
            for (int ch = 0; ch < deviceVector.Length; ch++)
            {
                resultVector[ch] = (byte)(deviceVector[ch] & 0xFF);
            }
            return resultVector;
        }
    }
    public class AnalogConverter : IConvert2<double[]>
    {
        //readonly double _peekToPeekVoltage;
        //readonly double _conversionFactor;
        double _peekVoltage_upstream;
        double _peekVoltage_downstream;

        public AnalogConverter( double peekVoltage_upstream, double peekVoltage_downstream)
        {
            _peekVoltage_upstream = peekVoltage_upstream;
            _peekVoltage_downstream = peekVoltage_downstream;
        }
        /// <summary>
        /// Convert from UInt16 to double
        /// </summary>
        public double [] DownstreamConvert(byte[] byteMessage)
        {
            double[] resultVector = new double[byteMessage.Length / sizeof(UInt16)];
            for (int chNum = 0; chNum < resultVector.Length; chNum++)
            {
                Int16 intval = BitConverter.ToInt16(byteMessage, 2 * chNum);
                resultVector[chNum] = Int16ToPlusMinusVoltage(_peekVoltage_downstream, intval);
            }
            return resultVector;
        }

        public byte[] UpstreamConvert(double [] dVector)
        {
            byte[] resultVector = new byte[dVector.Length * 2];
            for (int chNum = 0; chNum < dVector.Length; chNum++)
            {
                Int16 i16 = PlusMinusVoltageToInt16( _peekVoltage_upstream, dVector[chNum]);
                byte[] bytes = BitConverter.GetBytes(i16);
                Array.Copy(bytes, 0, resultVector, 2*chNum, bytes.Length);
            }
            return resultVector;
        }

        public static UInt16 PlusMinusVoltageToUInt16_old(double peekVoltage, double valueToConvert)
        {
            double v1 = (valueToConvert > peekVoltage) ? peekVoltage : valueToConvert;
            double v2 = (v1 < -peekVoltage) ? -peekVoltage : v1;

            double v3 = (v2 + peekVoltage) / peekVoltage / 2.0;
            double max = Convert.ToDouble(UInt16.MaxValue);
            UInt16 result = Convert.ToUInt16(v3 * max);
            return result;
        }
        static double Int16MaxValue = Convert.ToDouble(Int16.MaxValue);
        static double Int16MinValue = Convert.ToDouble(Int16.MinValue);
        public static Int16 PlusMinusVoltageToInt16(double peekVoltage, double dVal)
        {
            if (dVal >= 0)
            {
                // clip
                double v1 = (dVal > peekVoltage) ? peekVoltage : dVal;
                // norm
                double v2 = v1 / peekVoltage;
                // convert
                Int16 result = Convert.ToInt16(v2 * Int16MaxValue);
                return result;
            }
            else
            {
                // clip
                double v1 = (dVal < -peekVoltage) ? -peekVoltage : dVal;
                // norm
                double v2 = v1 / peekVoltage * Int16MinValue;
                // convert
                Int16 result = Convert.ToInt16(-v2);
                return result;

            }
        }

        public static double Uint16ToPlusMinusVoltage_old(double peekVoltage, UInt16 u16)
        {

            double d16 = Convert.ToDouble(u16);
            double dmax = Convert.ToDouble(UInt16.MaxValue);
            double r = (d16 / dmax * peekVoltage * 2.0);
            return (r - peekVoltage);
        }
        public static double Int16ToPlusMinusVoltage(double peekVoltage, Int16 i16)
        {
            if (i16 >= 0)
            {
                double d16 = Convert.ToDouble(i16);
                double r = (d16 / Int16MaxValue * peekVoltage);
                return r;
            }
            else
            {
                double d16 = Convert.ToDouble(i16);
                double r = (d16 / Int16MinValue * peekVoltage);
                return -r;
            }
        }
    }
}
