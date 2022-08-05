using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace UeiBridge
{
    //class Converters
    //{
    //    public static double[] AO308Convert(EthernetMessage mo)
    //    {
    //        log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);
    //        Debug.Assert(mo.PayloadBytes.Length  >= 16, "analog-out message too short");

    //        const int numberOfChannels = 8;
    //        const double factor = 20.0 / (Int16.MaxValue - Int16.MinValue);
    //        double[] v = new double[numberOfChannels];
    //        for (int chNum=0; chNum<numberOfChannels; chNum++)
    //        {
    //            int startIndex = 2 * chNum;
    //            Int16 ival = BitConverter.ToInt16(mo.PayloadBytes, startIndex);
    //            v[chNum] = ival * factor;
    //        }
    //        return v;
    //    }
    //}
    class AO308Convert : IConvert
    {
        public string DeviceName => "AO-308";
        string _lastError { get; set; }
        string IConvert.LastErrorMessage => _lastError;

        public byte[] DeviceToEth(object dt)
        {
            throw new NotImplementedException();
        }
        public object EthToDevice(byte[] messagePayload)
        {
            const int numberOfChannels = 8; // from icd
            double peekToPeekVoltage = Config.Instance.AnalogOutMinMaxVoltage.Item2 - Config.Instance.AnalogOutMinMaxVoltage.Item1;
            double factor = peekToPeekVoltage / (Int16.MaxValue - Int16.MinValue);

            if ((messagePayload.Length) < numberOfChannels * sizeof(UInt16))
            {
                _lastError = $"analog-out message too short. {messagePayload.Length} ";
                return null;
            }
            
            double[] resultVector = new double[numberOfChannels];
            for (int chNum = 0; chNum < numberOfChannels; chNum++)
            {
                Int16 ival = BitConverter.ToInt16(messagePayload, 2 * chNum);
                resultVector[chNum] = ival * factor;
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
            // tbd !!!
            UInt32[] result = { 1 };
            return result;
        }
    }
    class DIO403Convert : IConvert
    {
        public string DeviceName => "DIO-403";
        string _lastError { get; set; }
        string IConvert.LastErrorMessage => _lastError;

        public byte[] DeviceToEth(object dt)
        {
            UInt16[] deviceData = (UInt16[])dt;
            byte[] resultVector = new byte[2 * deviceData.Length];
            Array.Clear(resultVector, 0, resultVector.Length);
            int rvIndex = 0;
            foreach( UInt16 val in deviceData)
            {
                Array.Copy(BitConverter.GetBytes(val), 0, resultVector, rvIndex, 2);
                rvIndex += 2;
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

            UInt16 [] result = new UInt16[2*numberOfChannels];// payload might be 48 bits long
            Array.Clear(result, 0, result.Length);
            
            result[0] = BitConverter.ToUInt16(messagePayload, 0);
            result[1] = messagePayload[2];
            
            return result;

        }
    }

    class AI201Converter : IConvert
    {
        public string DeviceName => "AI-201-100";
        string _lastError { get; set; }
        string IConvert.LastErrorMessage => _lastError;

        public byte[] DeviceToEth(object dt)
        {
            const double peekToPeekVoltage = 24;
            const double factor = peekToPeekVoltage / (Int16.MaxValue - Int16.MinValue);

            double[] inputVector = dt as double[];
            System.Diagnostics.Debug.Assert(null != inputVector);
            byte[] resultVector = new byte[inputVector.Length * 2];
            int ch = 0;
            foreach (double val in inputVector)
            {
                UInt16 r = Convert.ToUInt16( val / factor);
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
