using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.IO;
using System.Net;
using UeiDaq;

/// <summary>
/// All classes in this file MUST NOT depend on any other module in the project
/// </summary>
namespace UeiBridge
{
    public class EndPoint
    {
        public string Address;
        public int Port;
        public EndPoint()
        {
        }
        public EndPoint(string addressString, int port)
        {
            Address = addressString;
            Port = port;
        }
        public IPEndPoint ToIpEp() // tbd. make cast operator
        {
            //try
            {
                IPEndPoint ipep = new IPEndPoint(IPAddress.Parse(Address), Port);
                return ipep;
            }
            //catch (Exception ex)
            //{
            //    return null;
            //}
        }
    }
    public class SerialChannel
    {
        [XmlAttribute("Port")]
        public string portname = "ComX";

        public SerialPortMode mode = SerialPortMode.RS232;
        [XmlElement("Baud")]
        public SerialPortSpeed Baudrate { get; set; }

        public SerialPortParity parity = SerialPortParity.None;
        public SerialPortStopBits stopbits = SerialPortStopBits.StopBits1;

        public SerialChannel(string portname, SerialPortSpeed speed)
        {
            this.portname = portname;
            //Baudrate = SerialPortSpeed.BitsPerSecond115200;
            Baudrate = speed;
        }
        public SerialChannel()
        {
        }
    }

    public class AppSetup
    {
        //[XmlElement(ElementName = "AppSetup")]
        public string SelectedNicForMCast = "221.109.251.103";
        public EndPoint StatusViewerEP = new EndPoint("239.10.10.17", 5093);
    }
    public class DeviceSetup
    {
        [XmlAttribute("Slot")]
        public int SlotNumber;
        public string DeviceName;
        public EndPoint LocalEndPoint;
        public EndPoint DestEndPoint;
        [XmlIgnore]
        public int SamplingInterval => 100; // ms

        public DeviceSetup(EndPoint localEndPoint, EndPoint destEndPoint, UeiDaq.Device device)
        {
            LocalEndPoint = localEndPoint;
            DestEndPoint = destEndPoint;
            DeviceName = device.GetDeviceName();
            SlotNumber = device.GetIndex();
        }
        /// <summary>
        /// This c-tor for block sensor which does not have a 'uei device'
        /// </summary>
        /// <param name="localEndPoint"></param>
        /// <param name="destEndPoint"></param>
        public DeviceSetup(EndPoint localEndPoint, EndPoint destEndPoint, string deviceName)
        {
            LocalEndPoint = localEndPoint;
            DestEndPoint = destEndPoint;
            DeviceName = deviceName;
        }
        protected DeviceSetup()
        {
        }
        private string cubeUrl;
        public string CubeUrl { get => cubeUrl; set => cubeUrl = value; }
    }
    public class BlockSensorSetup : DeviceSetup
    {
        public bool IsActive { get; set; }
        public BlockSensorSetup(EndPoint localEndPoint, string deviceName) : base(localEndPoint, null, deviceName)
        {
        }

        protected BlockSensorSetup()
        {
        }
    }
    public class AO308Setup : DeviceSetup
    {
        [XmlIgnore]
        public static double PeekVoltage_downstream => 10.0;

        public AO308Setup()
        {
        }
        public AO308Setup(EndPoint localEndPoint, UeiDaq.Device device) : base(localEndPoint, null, device)
        {
        }
    }
    public class SimuAO16Setup : DeviceSetup
    {
        public SimuAO16Setup()
        {
        }
        public SimuAO16Setup(EndPoint localEndPoint, UeiDaq.Device device) : base(localEndPoint, null, device)
        {
        }
    }
    public class AI201100Setup : DeviceSetup
    {
        [XmlIgnore]
        public static double PeekVoltage_upstream => 12.0;
        public AI201100Setup( EndPoint destEndPoint, Device device) : base( null, destEndPoint, device)
        {
        }
        protected AI201100Setup()
        {
        }
    }
    public class DIO403Setup : DeviceSetup
    {
        //public Direction[] BitOctets;
        public DIO403Setup()
        {
        }

        public DIO403Setup(EndPoint localEndPoint, EndPoint destEndPoint, UeiDaq.Device device) : base(localEndPoint, destEndPoint, device)
        {
        }
    }
    public class DIO470Setup : DeviceSetup
    {
        public DIO470Setup()
        {
        }

        public DIO470Setup(EndPoint localEndPoint, EndPoint destEndPoint, UeiDaq.Device device) : base(localEndPoint, destEndPoint, device)
        {
        }
    }
    public class SL508892Setup: DeviceSetup
    {
        public SerialChannel[] Channels;

        public SL508892Setup()
        {
        }

        public SL508892Setup(EndPoint localEndPoint, EndPoint destEndPoint, Device device) : base(localEndPoint, destEndPoint, device)
        {
            Channels = new SerialChannel[8];

            for (int ch = 0; ch < Channels.Length; ch++)
            {
                Channels[ch] = new SerialChannel($"Com{ch}", SerialPortSpeed.BitsPerSecond57600);
            }
        }
    }

    public static class ConfigFactory
    {
        static int portNumber=50035;
        public static string LocalIP => "227.3.1.10";
        public static string RemoteIp => "227.2.1.10";

        public static DeviceSetup DeviceSetupFactory( Device ueiDevice)
        {
            DeviceSetup result=null;

            switch (ueiDevice.GetDeviceName())
            {
                case "AO-308":
                    result = new AO308Setup( new EndPoint( LocalIP, portNumber++), ueiDevice);
                    break;
                case "DIO-403":
                    result = new DIO403Setup(new EndPoint( LocalIP, portNumber++), new EndPoint(RemoteIp, portNumber++), ueiDevice);
                    break;
                case "DIO-470":
                    result = new DIO470Setup(new EndPoint( LocalIP, portNumber++), new EndPoint(RemoteIp, portNumber++), ueiDevice);
                    break;
                case "AI-201-100":
                    result = new AI201100Setup(new EndPoint( RemoteIp, portNumber++), ueiDevice);
                    break;
                case "SL-508-892":
                    result = new SL508892Setup(new EndPoint( LocalIP, portNumber++), new EndPoint(RemoteIp, portNumber++), ueiDevice);
                    break;
                case "Simu-AO16":
                    result = new SimuAO16Setup(new EndPoint(LocalIP, portNumber++), ueiDevice);
                    break;
                default:
                    Console.WriteLine($"Config: Missing setup-class for device {ueiDevice.GetDeviceName()}");
                    result = new DeviceSetup(null, null, ueiDevice);
                    break;
            }

            return result;
        }
    }

    public class CubeSetup
    {
        //const int tbd = 7;
        [XmlAttribute("Url")]
        public string CubeUrl { get; set; }
        [XmlIgnore]
        public int CubeNumber=0;
        public List<DeviceSetup> DeviceSetupList = new List<DeviceSetup>();

        //private List<Device> _ueiDeviceList;

        public CubeSetup()
        {
        }
        /// <summary>
        /// Build DeviceSetupList
        /// </summary>
        public CubeSetup(string cubeUrl)
        {
            CubeUrl = cubeUrl;
            //CubeNumber = cubeNumber;
            //_ueiDeviceList = StaticMethods.GetDeviceList();
            DeviceCollection devColl = new DeviceCollection(cubeUrl);
            //DeviceSetupList = new DeviceSetup[devColl.Count];
            foreach (Device dev in devColl)
            {
                if (null == dev)
                    continue;

                DeviceSetupList.Add( ConfigFactory.DeviceSetupFactory(dev));
                int li = DeviceSetupList.Count - 1;
                var last = DeviceSetupList[li];
                System.Diagnostics.Debug.Assert(last != null);
                System.Diagnostics.Debug.Assert(li == dev.GetIndex());
            }
        }
    }
    [XmlInclude(typeof(AO308Setup))]
    [XmlInclude(typeof(DIO403Setup))]
    [XmlInclude(typeof(DIO470Setup))]
    [XmlInclude(typeof(AI201100Setup))]
    [XmlInclude(typeof(SL508892Setup))]
    [XmlInclude(typeof(BlockSensorSetup))]
    [XmlInclude(typeof(ValidValuesClass))]
    [XmlInclude(typeof(SimuAO16Setup))]
    public class Config2
    {
        private static Config2 _instance;
        private static object lockObject = new object();

        public AppSetup AppSetup;
        //public string[] CubeUrlList = new string[1];
        public CubeSetup[] UeiCubes;
        public BlockSensorSetup Blocksensor = new BlockSensorSetup(new EndPoint(ConfigFactory.LocalIP, 50105), "BlockSensor");
        public ValidValuesClass ValidValues = new ValidValuesClass();
        public static string SettingsFilename => "UeiSettings2.config";
        string[] _cubeUrls;
        private Config2()
        {
        }
        private Config2(string [] cubeUrls)
        {
            AppSetup = new AppSetup();
            UeiCubes = new CubeSetup[cubeUrls.Length];
            for (int i = 0; i < cubeUrls.Length; i++)
            {
                UeiCubes[i] = new CubeSetup(cubeUrls[i]);
            }
        }
        public static Config2 Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (lockObject)
                    {
                        if (_instance == null)
                            _instance = LoadConfig();
                    }
                }
                return _instance;
            }
        }
        public static bool IsConfigFileExist()
        {
            return System.IO.File.Exists(SettingsFilename);
        }
        public static void Reset()
        {
            _instance = null;
        }
        public void BuildNewConfig(string [] urls)
        {
            _cubeUrls = urls;

            var resultConfig = new Config2(urls); // default);

            //resultConfig = new Config2("simu://"); // default);
            var serializer = new XmlSerializer(typeof(Config2));
            using (var writer = new StreamWriter(SettingsFilename))
            {
                serializer.Serialize(writer, resultConfig);
                Console.WriteLine($"New default settings file created. {SettingsFilename}");
            }

            _instance = resultConfig;
        }
        private static Config2 LoadConfig()
        {
            
            var serializer = new XmlSerializer(typeof(Config2));
            Config2 resultConfig = null;
            if (System.IO.File.Exists( SettingsFilename))
            {
                try
                {
                    using (StreamReader sr = File.OpenText( SettingsFilename))
                    {
                        resultConfig = serializer.Deserialize(sr) as Config2;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to read settings file {SettingsFilename}. {ex.Message}");
                    Console.WriteLine($"Using default settings");
                    Console.WriteLine("For auto-create of default settings file, delete existing file and run program.");
                }
            }
            else
            {
                return new Config2();
            }
            //{
            //    throw new NotImplementedException();
            //    // make fresh config and write it to file
            //    resultConfig = new Config2("pdna://192.168.100.2/"); // default);
            //    //resultConfig = new Config2("simu://"); // default);
            //    using (var writer = new StreamWriter(filename))
            //    {
            //        serializer.Serialize(writer, resultConfig);
            //        Console.WriteLine($"New default settings file created. {filename}");
            //    }
            //}

            return resultConfig;
        }

        public DeviceSetup GetSetupEntryForDevice(int cubeId, string deviceName)
        {
            var ds = this.UeiCubes[0].DeviceSetupList.Where(i => i.DeviceName == deviceName);
            return ds.FirstOrDefault();
        }
        public DeviceSetup GetSetupEntryForDevice(string cubeUrl, int slotNumber)
        {
            if (this.UeiCubes == null)
            {
                return null;
            }
            var cubes = this.UeiCubes.Where(e => e.CubeUrl == cubeUrl);
            var selectedCube = cubes.FirstOrDefault();
            var slots = selectedCube.DeviceSetupList.Where(d => d.SlotNumber == slotNumber);
            return slots.FirstOrDefault();
        }

    }
    public class ValidValuesClass
    {
        public string ValidSerialModes;
        public string ValidBaudRates;
        public string ValidStopBitsValues;
        public string ValidParityValues;

        public ValidValuesClass()
        {
            ValidSerialModes = StaticMethods.GetEnumValues<SerialPortMode>();
            ValidBaudRates = StaticMethods.GetEnumValues<SerialPortSpeed>();
            ValidStopBitsValues = StaticMethods.GetEnumValues<SerialPortStopBits>();
            ValidParityValues = StaticMethods.GetEnumValues<SerialPortParity>();
        }
    }
}
