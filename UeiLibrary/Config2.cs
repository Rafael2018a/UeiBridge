using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.IO;
using System.Net;

//using UeiDaq;

/// <summary>
/// All classes in this file MUST NOT depend on any other module in the project
/// </summary>
namespace UeiBridge.Library
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
        
        public UeiDaq.SerialPortMode mode = UeiDaq.SerialPortMode.RS232;
        [XmlElement("Baud")]
        public UeiDaq.SerialPortSpeed Baudrate { get; set; }

        public UeiDaq.SerialPortParity parity = UeiDaq.SerialPortParity.None;
        public UeiDaq.SerialPortStopBits stopbits = UeiDaq.SerialPortStopBits.StopBits1;

        public SerialChannel(string portname, UeiDaq.SerialPortSpeed speed)
        {
            this.portname = portname;
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
        [XmlIgnore]
        public string CubeUrl { get; set; } // tbd. this is a patch.
        [XmlIgnore]
        public int CubeId { get; set; } // lsb of cube address
        [XmlIgnore]
        public bool IsBlockSensorActive { get; set; }

        public DeviceSetup(EndPoint localEndPoint, EndPoint destEndPoint, UeiDeviceAdapter device)
        {
            LocalEndPoint = localEndPoint;
            DestEndPoint = destEndPoint;
            DeviceName = device.DeviceName;
            SlotNumber = device.DeviceSlot;
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
        //private string cubeUrl;
        //public string CubeUrl { get => cubeUrl; set => cubeUrl = value; }
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
        public AO308Setup(EndPoint localEndPoint, UeiDeviceAdapter device) : base(localEndPoint, null, device)
        {
        }
    }
    public class SimuAO16Setup : DeviceSetup
    {
        public SimuAO16Setup()
        {
        }
        public SimuAO16Setup(EndPoint localEndPoint, UeiDeviceAdapter device) : base(localEndPoint, null, device)
        {
        }
    }
    public class AI201100Setup : DeviceSetup
    {
        [XmlIgnore]
        public static double PeekVoltage_upstream => 12.0;
        public AI201100Setup( EndPoint destEndPoint, UeiDeviceAdapter device) : base( null, destEndPoint, device)
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

        public DIO403Setup(EndPoint localEndPoint, EndPoint destEndPoint, UeiDeviceAdapter device) : base(localEndPoint, destEndPoint, device)
        {
        }
    }
    public class DIO470Setup : DeviceSetup
    {
        public DIO470Setup()
        {
        }

        public DIO470Setup(EndPoint localEndPoint, EndPoint destEndPoint, UeiDeviceAdapter device) : base(localEndPoint, destEndPoint, device)
        {
        }
    }
    public class SL508892Setup: DeviceSetup
    {
        public SerialChannel[] Channels;

        public SL508892Setup()
        {
        }

        public SL508892Setup(EndPoint localEndPoint, EndPoint destEndPoint, UeiDeviceAdapter device) : base(localEndPoint, destEndPoint, device)
        {
            Channels = new SerialChannel[8];

            for (int ch = 0; ch < Channels.Length; ch++)
            {
                Channels[ch] = new SerialChannel($"Com{ch}", UeiDaq.SerialPortSpeed.BitsPerSecond57600);
            }
        }
    }

    public static class ConfigFactory
    {
        static int portNumber=50035;
        public static string LocalIP => "227.3.1.10";
        public static string RemoteIp => "227.2.1.10";

        public static DeviceSetup DeviceSetupFactory( UeiDeviceAdapter ueiDevice)
        {
            DeviceSetup result=null;
            if (null == ueiDevice.DeviceName)
                throw new ArgumentNullException("device name");

            switch (ueiDevice.DeviceName)
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
                    Console.WriteLine($"Config: Device {ueiDevice.DeviceName} not supported.");
                    //result = new DeviceSetup(null, null, ueiDevice);
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

        public CubeSetup(List<UeiDeviceAdapter> deviceList, string cubeUrl)
        {
            CubeUrl = cubeUrl;

            foreach (var dev in deviceList)
            {
                if (null == dev)
                    throw new ArgumentNullException("null device");

                var dv = ConfigFactory.DeviceSetupFactory(dev);
                if (null != dv)
                {
                    DeviceSetupList.Add( dv);
                }
            }
        }
        /// <summary>
        /// Build DeviceSetupList
        /// </summary>
#if Obsolete
        public CubeSetup(string cubeUrl)
        {
            CubeUrl = cubeUrl;
            //CubeNumber = cubeNumber;
            //_ueiDeviceList = StaticMethods.GetDeviceList();
            UeiDaq.DeviceCollection devColl = new UeiDaq.DeviceCollection(cubeUrl); // tbd. cubesetup should not depend on ueidaq.
            try
            {
                //DeviceSetupList = new DeviceSetup[devColl.Count];
                foreach (UeiDaq.Device dev in devColl)
                {
                    if (null == dev)
                        continue;

                    var dv = ConfigFactory.DeviceSetupFactory(new UeiDeviceAdapter(dev));
                    if (dv != null)
                    {
                        DeviceSetupList.Add(dv);
                        //int li = DeviceSetupList.Count - 1;
                        //var last = DeviceSetupList[li];
                        //System.Diagnostics.Debug.Assert(last != null);
                        //System.Diagnostics.Debug.Assert(li == dev.GetIndex());
                    }
                }
            }
            catch(UeiDaq.UeiDaqException ex)
            {
                throw;
            }
        }
#endif
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
        //private static Config2 _instance;
        //private static object lockObject = new object();

        public AppSetup AppSetup;
        //public string[] CubeUrlList = new string[1];
        public List<CubeSetup> UeiCubes = new List<CubeSetup>();
        public BlockSensorSetup Blocksensor = new BlockSensorSetup(new EndPoint(ConfigFactory.LocalIP, 50105), "BlockSensor");
        public ValidValuesClass ValidValues = new ValidValuesClass();
        public static string DafaultSettingsFilename => "UeiSettings2.config";
        public static string SettingsFilename { get; private set; } = DafaultSettingsFilename;
        string[] _cubeUrls;
        //private CubeSetup[] cubeSetups;

        public Config2() // this is for serialization. 
        {
        }
#if        Obsolete
        private Config2(string [] cubeUrls)
        {
            AppSetup = new AppSetup();
            UeiCubes = new List<CubeSetup>( new CubeSetup[cubeUrls.Length]);
            for (int i = 0; i < cubeUrls.Length; i++)
            {
                UeiCubes[i] = new CubeSetup(cubeUrls[i]);
            }
        }
#endif
        public Config2(List<CubeSetup> cubeSetups)
        {
            AppSetup = new AppSetup();
            this.UeiCubes.AddRange(cubeSetups);
        }

        //public static Config2 Instance { get; set; }
        //{
        //get
        //{
        //    if (_instance == null)
        //    {
        //        lock (lockObject)
        //        {
        //            if (_instance == null)
        //                _instance = LoadConfigFromFile(SettingsFilename);
        //        }
        //    }
        //    return _instance;
        //}
        //}
        //public static bool IsConfigFileExist()
        //{
        //    return System.IO.File.Exists(SettingsFilename);
        //}
#if old
        public static void Reset()
        {
            _instance = null;
        }
        public void BuildNewConfig1(string [] urls)
        {
            _cubeUrls = urls;

            var resultConfig = new Config2(urls);

            var serializer = new XmlSerializer(typeof(Config2));
            using (var writer = new StreamWriter(SettingsFilename))
            {
                serializer.Serialize(writer, resultConfig);
                Console.WriteLine($"New default settings file created. {SettingsFilename}");
            }

            _instance = resultConfig;
        }
#endif
        /// <summary>
        /// Load config from file
        /// </summary>
        /// <returns></returns>
        public static Config2 LoadConfigFromFile( string configfilename)
        {
            var serializer = new XmlSerializer(typeof(Config2));
            Config2 resultConfig = null;
            if (System.IO.File.Exists(configfilename))
            {
                try
                {
                    using (StreamReader sr = File.OpenText(configfilename))
                    {
                        resultConfig = serializer.Deserialize(sr) as Config2;
                    }
                    return resultConfig;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to read settings file {configfilename}. {ex.Message}");
                    Console.WriteLine($"Using default settings");
                    Console.WriteLine("For auto-create of default settings file, delete existing file and run program.");
                    return null;
                }
            }

            return null;
            
        }
        public static Config2 BuildDefaultConfig( List<string> cubeUrlList) 
        {
            List<CubeSetup> csetupList = new List<CubeSetup>();
            foreach (var url in cubeUrlList)
            {
                UeiDaq.DeviceCollection devColl = new UeiDaq.DeviceCollection(url);
                List<UeiDeviceAdapter> rl = new List<UeiDeviceAdapter>();
                foreach (UeiDaq.Device dev in devColl)
                {
                    if (dev == null) continue; // this for the last entry, which is null
                    rl.Add(new UeiDeviceAdapter(dev.GetDeviceName(), dev.GetIndex()));
                }
                csetupList.Add(new CubeSetup(rl, url));
            }

            Config2 res = new Config2(csetupList);
            return res;

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
            var cube = this.UeiCubes.Where(e => e.CubeUrl == cubeUrl);
            var selectedCube = cube.FirstOrDefault();
            if (null==selectedCube)
            {
                return null;
            }
            var theSetups = selectedCube.DeviceSetupList.Where(d => d.SlotNumber == slotNumber);
            DeviceSetup result = theSetups.FirstOrDefault();
            result.CubeUrl = selectedCube.CubeUrl;
            IPAddress ip = StaticMethods.CubeUriToIpAddress(selectedCube.CubeUrl);
            result.CubeId = (null == ip)? -1: ip.GetAddressBytes()[3];
            return result;
        }

        public void SaveAs(string filename)
        {
            var serializer = new XmlSerializer(typeof(Config2));
            using (var writer = new StreamWriter(filename))
            {
                serializer.Serialize(writer, this);
               
            }
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
            ValidSerialModes = UeiBridge.Library.StaticMethods.GetEnumValues<UeiDaq.SerialPortMode>();
            ValidBaudRates = UeiBridge.Library.StaticMethods.GetEnumValues<UeiDaq.SerialPortSpeed>();
            ValidStopBitsValues = UeiBridge.Library.StaticMethods.GetEnumValues<UeiDaq.SerialPortStopBits>();
            ValidParityValues = UeiBridge.Library.StaticMethods.GetEnumValues<UeiDaq.SerialPortParity>();
        }
    }
}
