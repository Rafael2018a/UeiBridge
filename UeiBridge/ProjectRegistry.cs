using System;
using System.Collections.Generic;

namespace UeiBridge
{
    internal class ProjectRegistry
    {
        Dictionary<int, string> _deviceMap = new Dictionary<int, string>();
        Dictionary<string, IConvert> _convertersMap = new Dictionary<string, IConvert>();
        Dictionary<string, OutputDevice> _deviceManagersMap = new Dictionary<string, OutputDevice>();

        private ProjectRegistry()
        {

        }
        static ProjectRegistry _instance = new ProjectRegistry();
        internal static ProjectRegistry Instance { get => _instance; }
        /// <summary>
        /// 1:"AI-204"
        /// </summary>
        public Dictionary<int, string> DeviceKeys { get => _deviceMap; }
        /// <summary>
        /// "Ai-204": <instnace>
        /// </summary>
        public Dictionary<string, IConvert> ConvertersDic { get => _convertersMap; }
        /// <summary>
        /// "Ai-204": <instnace>
        /// </summary>
        public Dictionary<string, OutputDevice> DeviceManagersDic { get => _deviceManagersMap; }
        internal void Establish()
        {
            log4net.ILog logger = StaticMethods.GetLogger();

            _deviceMap.Add(0, "AO-308");
            _deviceMap.Add(4, "DIO-403");
            _deviceMap.Add(6, "DIO-430");
            _deviceMap.Add(1, "AI-201-100");

            // fill convertes and device-managers map
            foreach (Type theType in this.GetType().Assembly.GetTypes())
            {
                if (theType.IsInterface || theType.IsAbstract)
                    continue;

                if (typeof(IConvert).IsAssignableFrom(theType))
                {
                    IConvert obj = (IConvert)Activator.CreateInstance(theType);
                    logger.Debug($"New instance: {obj.ToString()} - {obj.DeviceName}");
                    _convertersMap.Add(obj.DeviceName, obj);
                }
                if (typeof(OutputDevice).IsAssignableFrom(theType))
                {
                    OutputDevice obj = (OutputDevice)Activator.CreateInstance(theType);
                    logger.Debug($"New instance: {obj.ToString()} - {obj.DeviceName}");
                    _deviceManagersMap.Add(obj.DeviceName, obj);

                }
            }

            //_convertersDic.Add("AO-308", new AO308Convert());
            //_convertersDic.Add("DIO-403", new DIO403Convert());
            //_convertersDic.Add("DIO-430", new DIO430Convert());

            //_deviceManagersDic.Add("AO-308", new AO308OutputDeviceManager());
            //_deviceManagersDic.Add("DIO-403", new DIO403OutputDeviceManager());
            //_deviceManagersDic.Add("DIO-430", new DIO430OutputDeviceManager());

            //_deviceManagersDic.Add(, new AI201InputDeviceManager());
        }
        /// <summary>
        /// </summary>
        /// <param name="deviceString"></param>
        /// <returns>-1 if key doesn't exist</returns>
        public int GetDeviceKeyFromDeviceString(string deviceString)
        {
            foreach (var pair in _deviceMap)
            {
                if (pair.Value == deviceString)
                {
                    return pair.Key;
                }
            }
            return -1;
        }
    }
}