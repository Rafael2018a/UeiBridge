using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UeiDaq;
//using UeiBridgeTypes;

namespace UeiBridge.Library
{
    public static class StaticMethods
    {
        static string _lastErrorMessage;
        public static string LastErrorMessage { get => _lastErrorMessage; }
        static Dictionary<int, string> _cardIdMap = new Dictionary<int, string>();

        static StaticMethods()
        {
            _cardIdMap.Add(0, "AO-308");
            _cardIdMap.Add(4, "DIO-403");
            _cardIdMap.Add(6, "DIO-470");
            _cardIdMap.Add(1, "AI-201-100");
            _cardIdMap.Add(5, "SL-508-892");
        }
        public static int GetCardIdFromCardName(string deviceName)
        {
            try
            {
                var p = _cardIdMap.ToList().Single(pair => pair.Value == deviceName);
                return p.Key;
            }
            catch (System.InvalidOperationException)
            {
                return -1;
            }
        }

        public static bool  DoesCardIdExist(int cardId)
        {
            return _cardIdMap.ContainsKey(cardId);
        }

        public static List<Device> GetDeviceList( string cubeUrl)
        {
            DeviceCollection devColl = new DeviceCollection(cubeUrl);
            List<Device> resultList = new List<Device>();
            foreach (Device dev in devColl)
            {
                if (dev != null)
                {
                    resultList.Add(dev);
                }
            }
            return resultList;
        }
        public static List<Device> GetDeviceList_notInUse( string deviceUrl)
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
        public static string FindDeviceIndex(string cubeUrl, string deviceName)
        {
            List<Device> devList = GetDeviceList( cubeUrl);
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

#if dont
        [Obsolete]
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
                    System.Diagnostics.Debug.Assert(oIinstnace.DeviceName.Length > 1);
                    if (oIinstnace.DeviceName == deviceSetup.DeviceName)
                        return oIinstnace;
                }
            }
            return null;
        }
#endif
        public static Type GetDeviceManagerType<ParentType>( string deviceName) where ParentType : IDeviceManager
        {
            foreach (Type theType in System.Reflection.Assembly.GetExecutingAssembly().GetTypes())
            {
                if (theType.IsInterface || theType.IsAbstract)
                    continue;

                // if theType is an OutputDevice class
                if (typeof(ParentType).IsAssignableFrom(theType))
                {
                    ParentType oIinstnace = (ParentType)Activator.CreateInstance(theType);
                    if (oIinstnace.DeviceName == deviceName)
                        return theType;
                    
                }
            }
            return null;
        }
        //public static void f()
        //{
        //    List<Type> lt = new List<Type>(System.Reflection.Assembly.GetExecutingAssembly().GetTypes());
        //    var sub = lt.Where(item => item == typeof(AO308Convert));
        //    Console.WriteLine(sub);
        //}


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
#if dont
        [Obsolete]
        public static Session CreateSerialSession( SL508892Setup deviceSetup)
        {
            System.Diagnostics.Debug.Assert(null != deviceSetup);
            Session serialSession = new Session();
            
            foreach (var channel in deviceSetup.Channels)
            {
                string finalUrl = $"{deviceSetup.CubeUrl}Dev{deviceSetup.SlotNumber}/{channel.portname}";
                var port = serialSession.CreateSerialPort(finalUrl,
                                    channel.mode,
                                    channel.Baudrate,
                                    SerialPortDataBits.DataBits8,
                                    channel.parity,
                                    channel.stopbits,
                                    "");
            }

            int numberOfChannels = serialSession.GetNumberOfChannels();
            //System.Diagnostics.Debug.Assert(numberOfChannels == Config.Instance.SerialChannels.Length);

            serialSession.ConfigureTimingForMessagingIO(1000, 100.0);
            serialSession.GetTiming().SetTimeout(5000); // timeout to throw from _serialReader.EndRead (looks like default is 1000)

            //serialSession.ConfigureTimingForSimpleIO();

            serialSession.Start();
            return serialSession;
        }
#endif

#if dont
        [Obsolete]
        public static Session CreateSerialSession2(SL508892Setup deviceSetup)
        {
            Session srSession = new Session();
            string finalUrl = $"{deviceSetup.CubeUrl}Dev{deviceSetup.SlotNumber}/com0:7";

            srSession.CreateSerialPort(finalUrl,
                SerialPortMode.RS485FullDuplex,
                SerialPortSpeed.BitsPerSecond57600,
                SerialPortDataBits.DataBits8,
                SerialPortParity.None,
                SerialPortStopBits.StopBits1,
                "");

            srSession.ConfigureTimingForMessagingIO(100, 10.0);
            srSession.GetTiming().SetTimeout(5);

            System.Diagnostics.Debug.Assert(8 == srSession.GetNumberOfChannels());

            srSession.Start();
            return srSession;
        }
#endif

    }
}
