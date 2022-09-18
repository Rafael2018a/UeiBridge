using System;
using System.Linq;
using System.Collections.Generic;

namespace UeiBridge
{
    internal class ProjectRegistry 
    {
        Dictionary<int, string> _deviceMap = new Dictionary<int, string>();
        Dictionary<string, OutputDevice> _deviceManagersMap = new Dictionary<string, OutputDevice>();
        static ProjectRegistry _instance = new ProjectRegistry();
        internal static ProjectRegistry Instance { get => _instance; }
        public SL508InputDeviceManager SerialDeviceManager { get; set; }
        /// <summary>
        /// 1:"AI-204"
        /// </summary>
        public Dictionary<int, string> DeviceKeys { get => _deviceMap; }
        /// <summary>
        /// "Ai-204": <instnace>
        /// </summary>
        public Dictionary<string, OutputDevice> DeviceManagersDic { get => _deviceManagersMap; } // output devices. (tbd: rename)
        internal void Establish() // tbd. refactor this.
        {
            log4net.ILog logger = log4net.LogManager.GetLogger("Root");

            _deviceMap.Add(0, "AO-308");
            _deviceMap.Add(4, "DIO-403");
            _deviceMap.Add(6, "DIO-430");
            _deviceMap.Add(1, "AI-201-100");
            _deviceMap.Add(5, "SL-508-892");

            // fill convertes and device-managers map
            foreach (Type theType in System.Reflection.Assembly.GetExecutingAssembly().GetTypes())
            {
                if (theType.IsInterface || theType.IsAbstract)
                    continue;

                if (typeof(OutputDevice).IsAssignableFrom(theType))
                {
                    OutputDevice obj = (OutputDevice)Activator.CreateInstance(theType);
                    logger.Debug($"New output device instance: {obj.ToString()} - {obj.DeviceName}");
                    _deviceManagersMap.Add(obj.DeviceName, obj);
                }
            }
        }
        public int GetDeviceKeyFromDeviceString(string deviceString)
        {
            try
            {
                var p = _deviceMap.ToList().Single(pair => pair.Value == deviceString);
                return p.Key;
            }
            catch(System.InvalidOperationException)
            {
                return -1;
            }
        }
        public UeiBridge.OutputDevice GetDeviceManagerInstance(string deviceString)
        {
            try
            {
                var p = _deviceManagersMap.ToList().Single( pair => pair.Key == deviceString);
                return p.Value;
            }
            catch (System.InvalidOperationException)
            {
                return null;
            }
        }
    }
}