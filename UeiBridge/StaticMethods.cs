﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UeiDaq;

namespace UeiBridge
{
    static class StaticMethods
    {
        static string _lastErrorMessage;
        public static string LastErrorMessage { get => _lastErrorMessage; }

        public static List<Device> GetDeviceList()
        {
            DeviceCollection devColl = new DeviceCollection(Config.Instance.DeviceUrl);
            List<Device> resultList = new List<Device>();
            try
            {
                foreach (Device dev in devColl)
                {
                    if (dev != null)
                    {
                        resultList.Add(dev);
                    }
                }
            }
            catch (Exception ex)
            {
                _lastErrorMessage = ex.Message;
                return null;
            }
            return resultList;
        }
        public static List<Device> GetDeviceList( string deviceUrl)
        {
            DeviceCollection devColl = new DeviceCollection(deviceUrl);
            List<Device> resultList = new List<Device>();
            try
            {
                foreach (Device dev in devColl)
                {
                    if (dev != null)
                    {
                        resultList.Add(dev);
                    }
                }
            }
            catch (Exception ex)
            {
                _lastErrorMessage = ex.Message;
                return null;
            }
            return resultList;
        }
        /// <summary>
        /// Example: input "DO-403", output "Dev0"
        /// return null if device not found
        /// </summary>
        public static string FindDeviceIndex(string deviceName)
        {
            List<Device> devList = GetDeviceList();
            var x = devList.Find(s => s.GetDeviceName() == deviceName);
            if (null != x)
            {
                string rc = "Dev" + x.GetIndex() + "/";
                return rc;
            }
            else
                return null;
        }

        public static log4net.ILog GetLogger()
        {
            //var m = System.Reflection.MethodBase.GetCurrentMethod();
            //var m1 = System.Reflection.MethodBase.GetCurrentMethod().

            System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace();
            // Get calling method name
            var m = stackTrace.GetFrame(1).GetMethod();


            var x = log4net.LogManager.GetLogger(m.DeclaringType.Name);
            //var x = log4net.LogManager.GetLogger("SpecialLogger");

            return x;
        }

        /// <summary>
        /// Find and instantiate suitalble converter
        /// </summary>
        /// <returns></returns>
        public static IConvert CreateConverterInstance(string deviceName)
        {
            IConvert attachedConverter = null;
            foreach (Type theType in System.Reflection.Assembly.GetExecutingAssembly().GetTypes())
            {
                if (theType.IsInterface || theType.IsAbstract)
                    continue;

                if (typeof(IConvert).IsAssignableFrom(theType))
                {
                    var at = (IConvert)Activator.CreateInstance(theType);
                    if (at.DeviceName == deviceName)
                    {
                        attachedConverter = at;
                        break;
                    }
                }
            }
            System.Diagnostics.Debug.Assert(attachedConverter != null);
            return attachedConverter;
        }

        public static OutputDevice CreateOutputDeviceManager(DeviceSetup deviceSetup)
        {
            //Config2.Instance.UeiCubes[0].SlotList
            foreach (Type theType in System.Reflection.Assembly.GetExecutingAssembly().GetTypes())
            {
                if (theType.IsInterface || theType.IsAbstract)
                    continue;

                // if theType is an OutputDevice class
                if (typeof(OutputDevice).IsAssignableFrom(theType))
                {
                    OutputDevice oIinstnace = (OutputDevice)Activator.CreateInstance(theType, deviceSetup);
                    if (oIinstnace.DeviceName == deviceSetup.DeviceName)
                        return oIinstnace;
                }
            }
            return null;
        }
        public static void f()
        {
            List<Type> lt = new List<Type>(System.Reflection.Assembly.GetExecutingAssembly().GetTypes());
            var sub = lt.Where(item => item == typeof(AO308Convert));
            Console.WriteLine(sub);
        }


        public static byte[] Make_A308Down_message()
        {
            EthernetMessage msg = EthernetMessage.CreateEmpty(0, 16);
            for(int i=0; i<16; i+=2)
            {
                msg.PayloadBytes[i] = (byte)(i * 10);
            }
            return msg.ToByteArrayDown();
        }
        public static byte[] Make_DIO403Down_Message()
        {
            EthernetMessage msg = EthernetMessage.CreateEmpty(4, 3);
            msg.PayloadBytes[0] = 0x12;
            msg.PayloadBytes[1] = 0x34;
            msg.PayloadBytes[2] = 0x56;

            return msg.ToByteArrayDown();
        }
        public static byte[] Make_DIO430Down_Message()
        {
            EthernetMessage msg = EthernetMessage.CreateEmpty(6, 16);
            return msg.ToByteArrayDown();
        }
        public static List<byte[]> Make_SL508Down_Messages( int seed)
        {
            List<byte[]> msgs = new List<byte[]>();

            // build 8 messages, one per channel
            for (int ch = 0; ch < 8; ch++)
            //int ch = 1;
            {
                string m = $"hello ch{ch} seed {seed} ksd klskd kljasldkjf laksjdfkl klsjd fkasdfjlk askldjfklasjdf asdfklj ksdajf ";

                // string to ascii

                // ascii to string System.Text.Encoding.ASCII.GetString(recvBytes)
                EthernetMessage msg = EthernetMessage.CreateEmpty(5, 16);
                msg.PayloadBytes = System.Text.Encoding.ASCII.GetBytes(m);
                msg.SlotChannelNumber = ch;
                msgs.Add(msg.ToByteArrayDown());

            }
            return msgs;
        }
        public static string GetEnumValues<T>()
        {
            T[] v1 = Enum.GetValues(typeof(T)) as T[];
            StringBuilder sb = new StringBuilder("\n");
            foreach (var item in v1)
            {
                sb.Append(item);
                sb.Append("\n");
            }
            //v1.ToList<SerialPortMode>().ForEach(item => { sb.Append(item); sb.Append("\n"); });
            return sb.ToString();
        }

    }
}
