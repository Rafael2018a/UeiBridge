using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UeiDaq;
using UeiBridge.Types;
using UeiBridge.Library;

namespace UeiBridge
{
    public static class StaticMethods
    {
        //static string _lastErrorMessage;
        //public static string LastErrorMessage { get => _lastErrorMessage; }
        

        static StaticMethods()
        {
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
        /// Find and instantiate suitable converter
        /// </summary>
        /// <returns></returns>
        public static IConvert CreateConverterInstance( DeviceSetup setup) // tbd. deviceName not needed
        {
            IConvert attachedConverter = null;
            foreach (Type theType in System.Reflection.Assembly.GetExecutingAssembly().GetTypes())
            {
                if (theType.IsInterface || theType.IsAbstract)
                    continue;

                if (typeof(IConvert).IsAssignableFrom(theType))
                {
                    var at = (IConvert)Activator.CreateInstance(theType, setup);
                    if (at.DeviceName == setup.DeviceName)
                    {
                        attachedConverter = at;
                        break;
                    }
                }
            }
            
            return attachedConverter;
        }

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

        /// <summary>
        /// Create EthernetMessage from device result.
        /// Might return null.
        /// </summary>
        public static EthernetMessage BuildEthernetMessageFromDevice(byte[] payload, DeviceSetup setup, int serialChannel = 0)
        {
            //ILog _logger = log4net.LogManager.GetLogger("Root");

            //int key = //ProjectRegistry.Instance.GetDeviceKeyFromDeviceString(deviceName);
            int key = DeviceMap2.GetCardIdFromCardName(setup.DeviceName);

            System.Diagnostics.Debug.Assert(key >= 0);

            EthernetMessage msg = new EthernetMessage();
            if (setup.GetType() == typeof(SL508892Setup))
            {
                msg.SerialChannelNumber = serialChannel;
            }

            msg.SlotNumber = setup.SlotNumber;
            msg.UnitId = 0; // tbd
            msg.CardType = (byte)key;
            msg.PayloadBytes = payload;

            System.Diagnostics.Debug.Assert(msg.CheckValid());

            return msg;
        }

    }
}
