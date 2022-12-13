using System;
using System.Linq;
using System.Collections.Generic;

namespace UeiBridge
{
    public enum CardFeature { Analog, Digital, Ralay, Serial};
    public enum CardType { AO308, DIO403, DIO470, AI201100, SL508892 }
    public enum Direction { input, output, in_out}



    internal class ProjectRegistry
    {
        Dictionary<int, string> _deviceMap = new Dictionary<int, string>();
        Dictionary<string, OutputDevice> _outputDeviceMap = new Dictionary<string, OutputDevice>();
        static ProjectRegistry _instance = new ProjectRegistry();
        internal static ProjectRegistry Instance { get => _instance; }
        public SL508InputDeviceManager SerialInputDeviceManager { get; set; }
        /// <summary>
        /// 1:"AI-204"
        /// </summary>
        public Dictionary<int, string> DeviceKeys { get => _deviceMap; }
        /// <summary>
        /// "Ai-204": <instnace>
        /// </summary>
        public Dictionary<string, OutputDevice> OutputDevicesMap { get => _outputDeviceMap; } // tbd: remove this
        public OutputDevice[] OutputDeviceList { get => _outputDeviceList; }
        public InputDevice[] InputDeviceList { get => _inputDeviceList; set => _inputDeviceList = value; }
        private OutputDevice[] _outputDeviceList = new OutputDevice[16]; // device by slot
        private InputDevice[] _inputDeviceList = new InputDevice[16];
        internal void BuildDeviceList()
        {
            // for each slot in config
            // - verify that slot-device identical to real device
            // - find device manager
            // - if it is an output device
            // -- create UdpReader for it
            // -- create and instance and add it to outputDeviceList
            // - if it is input device (might be both)
            // -- 
        }
        internal void Establish() // tbd. refactor this.
        {
            log4net.ILog logger = StaticMethods.GetLogger();

            _deviceMap.Add(0, "AO-308");
            _deviceMap.Add(4, "DIO-403");
            _deviceMap.Add(6, "DIO-430");
            _deviceMap.Add(1, "AI-201-100");
            _deviceMap.Add(5, "SL-508-892");

            // fill output device map
            //foreach (Type theType in System.Reflection.Assembly.GetExecutingAssembly().GetTypes())
            //{
            //    if (theType.IsInterface || theType.IsAbstract)
            //        continue;

            //    // if theType is an OutputDevice class
            //    if (typeof(OutputDevice).IsAssignableFrom(theType))
            //    {
            //        // add device to device map
            //        OutputDevice obj = (OutputDevice)Activator.CreateInstance(theType);
            //        _outputDeviceMap.Add(obj.DeviceName, obj);
            //    }
            //}
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
                var p = _outputDeviceMap.ToList().Single( pair => pair.Key == deviceString);
                return p.Value;
            }
            catch (System.InvalidOperationException)
            {
                return null;
            }
        }
    }
}