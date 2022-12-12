using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Serialization;
using System.Net;
using UeiDaq;

namespace UeiBridge
{
    public class SerialChannel
    {
        public string portname = "ComX";
        public SerialPortMode mode = SerialPortMode.RS232;
        public SerialPortSpeed baudrate = SerialPortSpeed.BitsPerSecond250000;

        public SerialPortParity parity = SerialPortParity.None;
        public SerialPortStopBits stopbits = SerialPortStopBits.StopBits1;

        public SerialChannel(string portname)
        {
            this.portname = portname;
        }
        public SerialChannel()
        {
        }
    }
        

    public class SlotSetup
    {
        public string CubeIp;
        public int SlotNumber;
        public Direction direction;
        public CardType CardId;
        public string UdpAddress;
        public int UdpdPort;
    }
    
    public class Config
    {
        public string DeviceUrl = "pdna://192.168.100.2/";
        
        public string ReceiverMulticastAddress = "227.3.1.10";
        public int ReceiverMulticastPort = 50035;
        public string SenderMulticastAddress = "227.2.1.10";
        public int SenderMulticastPort = 50038;
        public string SelectedNicForMcastSend = "221.109.251.103";

        readonly int _maxDigital403OutputChannels = 3; // each channel contains 8 bits

        readonly int _maxAnalogOutputChannels = 8;
        readonly double _analog_Out_PeekVoltage = 10.0;
        readonly double _analog_In_PeekVoltage = 12.0;

        private EndPoint[] LocalMcastEndPoints;
        private EndPoint[] DestMcastEndPoints;
        public SlotSetup[] SlotsSetup;
        public SerialChannel[] SerialChannels = new SerialChannel[8];
        //public object [] varList = new object[3];


        public string ValidSerialModes;
        public string ValidBaudRates;
        public string ValidStopBitsValues;
        public string ValidParityValues;

        private static volatile Config _instance;
        private static object syncRoot = new object();
        private Config()
        {
            for (int i = 0; i < SerialChannels.Length; i++)
                SerialChannels[i] = new SerialChannel("Com" + i.ToString());

            ValidSerialModes = StaticMethods.GetEnumValues<SerialPortMode>();
            ValidBaudRates = StaticMethods.GetEnumValues<SerialPortSpeed>();
            ValidStopBitsValues = StaticMethods.GetEnumValues<SerialPortStopBits>();
            ValidParityValues = StaticMethods.GetEnumValues<SerialPortParity>();

            LocalMcastEndPoints = new EndPoint[Enum.GetNames(typeof(CardFeature)).Length];
            DestMcastEndPoints = new EndPoint[Enum.GetNames(typeof(CardFeature)).Length];
            SlotsSetup = new SlotSetup[10];

            for (int i = 0; i < LocalMcastEndPoints.Length; i++)
            {
                LocalMcastEndPoints[i] = new EndPoint();
                LocalMcastEndPoints[i].Address = "227.3.1.10";
                LocalMcastEndPoints[i].Port = 50035 + i;
                //LocalMcastEndPoints[i].CardFeature = (CardFeature)i;

                DestMcastEndPoints[i] = new EndPoint();
                DestMcastEndPoints[i].Address = "227.2.1.10";
                DestMcastEndPoints[i].Port += 50135 + i;
                //DestMcastEndPoints[i].CardFeature = (CardFeature)i;
            }
         
            for(int i=0; i<SlotsSetup.Length; i++)
            {
                SlotsSetup[i] = new SlotSetup();
                SlotsSetup[i].UdpAddress = "227.3.1.10";
                SlotsSetup[i].UdpdPort = 30025 + i;
                SlotsSetup[i].CubeIp = "pdna://192.168.100.2/";
                SlotsSetup[i].CardId = CardType.AO308;
                SlotsSetup[i].direction = Direction.input;
                SlotsSetup[i].SlotNumber = i;
            }

            //varList[0] = new SerialChannel("portnamexxx");
            //varList[1] = "Alon";
            //varList[2] = new DigitalCard1();
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

 
