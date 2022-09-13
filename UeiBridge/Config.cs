using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Serialization;

namespace UeiBridge
{
    public class Config1
    {
        public int one = 1;
        public string two = "hi";
    }

    public class SerialChannel
    {
        public string portname = "port1";
    }
    public class Config
    {
        //static Config _instance = new Config();
        public int LocalPort = 50035;
        public string DeviceUrl = "pdna://192.168.100.2/";
        //public Tuple<double, double> _analog_Out_MinMaxVoltage = new Tuple<double, double>( -10.0, 10.0 );
        //readonly Tuple<double, double> _analog_In_MinMaxVoltage = new Tuple<double, double>(-15.0, 15.0); // -15,15 means 'no gain'
        readonly double _analog_Out_PeekVoltage = 10.0;
        readonly double _analog_In_PeekVoltage = 12.0;
        readonly string _receiverMulticastAddress = "227.3.1.10";
        readonly string _destMulticastAddress = "227.2.1.10";
        readonly int _destMulticastPort = 50038;
        readonly string _localBindNicAddress = "221.109.251.103";
        readonly int _maxAnalogOutputChannels = 8;
        readonly int _maxDigital403OutputChannels = 3; // each channel contains 8 bits

        public SerialChannel[] ser1 = new SerialChannel[8];

        private static volatile Config _instance;
        private static object syncRoot = new object();

        internal static Config LoadConfig()
        {
            string filename = "UeiSettings.config";
            var serializer = new XmlSerializer( typeof(Config));
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
                catch(Exception ex)
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

            if (null==resultConfig)
            {
                resultConfig = new Config();
            }

            return resultConfig;
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

        private Config()
        {
            for (int i = 0; i < ser1.Length; i++)
                ser1[i] = new SerialChannel();
            ser1[0].portname = "Port A";
        }
        //internal static Config Instance { get => _instance; }
        
        //public Tuple<double, double> Analog_Out_MinMaxVoltage => _analog_Out_MinMaxVoltage;
        //public Tuple<double, double> Analog_In_MinMaxVoltage => _analog_In_MinMaxVoltage;
        public string ReceiverMulticastAddress => _receiverMulticastAddress;
        public string DestMulticastAddress => _destMulticastAddress;
        public int DestMulticastPort => _destMulticastPort;
        public string LocalBindNicAddress => _localBindNicAddress;
        public double Analog_Out_PeekVoltage => _analog_Out_PeekVoltage; // tbd. use this
        public double Analog_In_PeekVoltage => _analog_In_PeekVoltage; // tbd. use this
        public int MaxAnalogOutputChannels => _maxAnalogOutputChannels;
        public int MaxDigital403OutputChannels => _maxDigital403OutputChannels;
    }
}

 
