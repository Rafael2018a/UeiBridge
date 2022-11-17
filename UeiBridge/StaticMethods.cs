using System;
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
        public static void f()
        {
            List<Type> lt = new List<Type>(System.Reflection.Assembly.GetExecutingAssembly().GetTypes());
            var sub = lt.Where(item => item == typeof(AO308Convert));
            Console.WriteLine(sub);
        }


        public static byte[] Make_A308Down_message()
        {
            EthernetMessage msg = EthernetMessageFactory.CreateEmpty(0, 16);
            return msg.ToByteArrayDown();
        }
        public static byte[] Make_DIO403Down_Message()
        {
            EthernetMessage msg = EthernetMessageFactory.CreateEmpty(4, 3);
            msg.PayloadBytes[0] = 0x12;
            msg.PayloadBytes[1] = 0x34;
            msg.PayloadBytes[2] = 0x56;

            return msg.ToByteArrayDown();
        }
        public static byte[] Make_DIO430Down_Message()
        {
            EthernetMessage msg = EthernetMessageFactory.CreateEmpty(6, 16);
            return msg.ToByteArrayDown();
        }
        public static List<byte[]> Make_SL508Down_Messages( int seed)
        {
            List<byte[]> msgs = new List<byte[]>();

            for (int ch = 0; ch < 2; ch++)
            //int ch = 1;
            {
                string m = $"hello ch{ch} seed {seed} jjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjj";

                // string to ascii

                // ascii to string System.Text.Encoding.ASCII.GetString(recvBytes)
                EthernetMessage msg = EthernetMessageFactory.CreateEmpty(5, 16);
                msg.PayloadBytes = System.Text.Encoding.ASCII.GetBytes(m);
                msg.SlotChannelNumber = ch;
                msgs.Add(msg.ToByteArrayDown());

            }

            return msgs;

        }
    }
}
