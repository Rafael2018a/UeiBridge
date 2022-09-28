using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Serialization;
using UeiDaq;

namespace UeiBridge
{
    public class SerialChannel
    {
        public string portname = "port1";
        public SerialPortMode portmode = SerialPortMode.RS485FullDuplex;
    }
    public class Config
    {
        public string DeviceUrl = "pdna://192.168.100.2/";
        
        public string ReceiverMulticastAddress = "227.3.1.10";
        public int ReceiverMulticastPort = 50035;
        public string SenderMulticastAddress = "227.2.1.10";
        public int SenderMulticastPort = 50038;
        public string LocalBindNicAddress = "221.109.251.103";

        readonly int _maxDigital403OutputChannels = 3; // each channel contains 8 bits

        readonly int _maxAnalogOutputChannels = 8;
        readonly double _analog_Out_PeekVoltage = 10.0;
        readonly double _analog_In_PeekVoltage = 12.0;

        public SerialChannel[] SerialChannels = new SerialChannel[8];
        private static volatile Config _instance;
        private static object syncRoot = new object();
        private Config()
        {
            for (int i = 0; i < SerialChannels.Length; i++)
                SerialChannels[i] = new SerialChannel();
        }

        public static Config Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (syncRoot)
                    {
                        if (_instance == null)
                            _instance = LoadConfig();
                    }
                }
                return _instance;
            }
        }

        internal static Config LoadConfig()
        {
            string filename = "UeiSettings.config";
            var serializer = new XmlSerializer(typeof(Config));
            Config resultConfig = null;
            if (System.IO.File.Exists(filename))
            {
                try
                {
                    using (StreamReader sr = File.OpenText(filename))
                    {
                        resultConfig = serializer.Deserialize(sr) as Config;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to read settings file {filename}. {ex.Message}");
                    Console.WriteLine($"Using default settings");
                    Console.WriteLine("For auto-create of default settings file, delete exising file and run program.");
                }
            }
            else
            {
                using (var writer = new StreamWriter(filename))
                {
                    serializer.Serialize(writer, new Config());
                    Console.WriteLine($"New default settings file created. {filename}");
                }
            }

            if (null == resultConfig)
            {
                resultConfig = new Config();
            }

            return resultConfig;
        }

        public double Analog_Out_PeekVoltage => _analog_Out_PeekVoltage; 
        public double Analog_In_PeekVoltage => _analog_In_PeekVoltage; 
        public int MaxAnalogOutputChannels => _maxAnalogOutputChannels;
        public int MaxDigital403OutputChannels => _maxDigital403OutputChannels;
    }
}

 
