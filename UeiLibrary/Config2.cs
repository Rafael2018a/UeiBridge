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
    public class EndPoint : IEquatable<EndPoint>
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

        public bool Equals(EndPoint other)
        {
            return (this.Address == other.Address) && (this.Port == other.Port);
        }

        public IPEndPoint ToIpEp() // should be cast operator
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
    public class SerialChannelSetup
    {
        [XmlAttribute("ChannelIndex")]
        public int ChannelIndex = -1;
        public UeiDaq.SerialPortMode mode = UeiDaq.SerialPortMode.RS232;
        //[XmlElement("Baud")]
        public UeiDaq.SerialPortSpeed Baudrate { get; set; }
        public UeiDaq.SerialPortParity parity = UeiDaq.SerialPortParity.None;
        public UeiDaq.SerialPortStopBits stopbits = UeiDaq.SerialPortStopBits.StopBits1;
        public int LocalUdpPort { get; set; }

        public SerialChannelSetup(int channelIndex, UeiDaq.SerialPortSpeed speed)
        {
            this.ChannelIndex = channelIndex;
            this.Baudrate = speed;
        }
        public SerialChannelSetup()
        {
        }
    }

    public class AppSetup
    {
        //[XmlElement(ElementName = "AppSetup")]
        public string SelectedNicForMCast = "221.109.251.103";
        public EndPoint StatusViewerEP = new EndPoint("239.10.10.17", 5093);
    }
    public class DeviceSetup : IEquatable<DeviceSetup>
    {
        [XmlAttribute("Slot")]
        public int SlotNumber;
        public string DeviceName;
        public EndPoint LocalEndPoint;
        public EndPoint DestEndPoint;
        [XmlIgnore]
        public int SamplingInterval => 100; // ms
        [XmlIgnore]
        public string CubeUrl { get; set; } 
        [XmlIgnore]
        public int CubeId // lsb of cube address
        {
            get
            {
                int result = -1;
                if (null != CubeUrl)
                {
                    IPAddress ipa = Config2.CubeUriToIpAddress(CubeUrl);
                    if (null != ipa)
                    {
                        result = ipa.GetAddressBytes()[3];
                    }
                }
                return result;
            }
        }

        public DeviceSetup(EndPoint localEndPoint, EndPoint destEndPoint, UeiDeviceInfo device)
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

        public bool Equals(DeviceSetup other)
        {
            //            public int SlotNumber;
            //public string DeviceName;
            //public EndPoint LocalEndPoint;
            //public EndPoint DestEndPoint;
            bool f1 = this.SlotNumber == other.SlotNumber;
            bool f2 = this.DeviceName == other.DeviceName;
            bool f3 = (this.LocalEndPoint == null) ? true : this.LocalEndPoint.Equals(other.LocalEndPoint);
            bool f4 = (this.DestEndPoint == null) ? true : this.DestEndPoint.Equals(other.DestEndPoint);

            return f1 && f2 && f3 && f4;
        }
        //private string cubeUrl;
        //public string CubeUrl { get => cubeUrl; set => cubeUrl = value; }
        public string GetInstanceName()
        {
            return $"{this.DeviceName}/Cube{this.CubeId}/Slot{this.SlotNumber}";
        }
        public UeiDeviceInfo GetDeviceInfo()
        {
            return new UeiDeviceInfo(CubeUrl, SlotNumber, DeviceName);
        }
    }
    
    public class BlockSensorSetup : DeviceSetup
    {
        public const int BlockSensorSlotNumber = 32;
        public bool IsActive { get; set; }
        public int AnalogCardSlot { get; set; }
        public int DigitalCardSlot { get; set; }
        public BlockSensorSetup(EndPoint localEndPoint, string deviceName) : base(localEndPoint, null, deviceName)
        {
            SlotNumber = AnalogCardSlot;
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
        public AO308Setup(EndPoint localEndPoint, UeiDeviceInfo device) : base(localEndPoint, null, device)
        {
        }
    }
    public class AO332Setup : DeviceSetup
    {
        [XmlIgnore]
        public static double PeekVoltage_downstream => 10.0;

        public AO332Setup()
        {
        }
        public AO332Setup(EndPoint localEndPoint, UeiDeviceInfo device) : base(localEndPoint, null, device)
        {
        }
    }
    //public class AO16Setup : AO308Setup { }
    public class SimuAO16Setup : DeviceSetup
    {
        public SimuAO16Setup()
        {
        }
        public SimuAO16Setup(EndPoint localEndPoint, UeiDeviceInfo device) : base(localEndPoint, null, device)
        {
        }
    }
    public class AI201100Setup : DeviceSetup
    {
        [XmlIgnore]
        public static double PeekVoltage_upstream => 12.0;
        public AI201100Setup(EndPoint destEndPoint, UeiDeviceInfo device) : base(null, destEndPoint, device)
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

        public DIO403Setup(EndPoint localEndPoint, EndPoint destEndPoint, UeiDeviceInfo device) : base(localEndPoint, destEndPoint, device)
        {
        }
    }
    public class DIO470Setup : DeviceSetup
    {
        public DIO470Setup()
        {
        }

        public DIO470Setup(EndPoint localEndPoint, UeiDeviceInfo device) : base(localEndPoint, null, device)
        {
        }
    }
    public class SL508892Setup : DeviceSetup
    {
        public List<SerialChannelSetup> Channels;
        const int _numberOfSerialChannels = 8;
        public SL508892Setup()
        {
        }

        public SL508892Setup(EndPoint localEndPoint, EndPoint destEndPoint, UeiDeviceInfo device) : base(localEndPoint, destEndPoint, device)
        {
            Channels = new List<SerialChannelSetup>();

            for (int chIndex = 0; chIndex < _numberOfSerialChannels; chIndex++)
            {
                Channels.Add( new SerialChannelSetup(chIndex, UeiDaq.SerialPortSpeed.BitsPerSecond19200));
            }
        }
    }

    public static class ConfigFactory
    {
        static int portNumber = 50035;
        public static string LocalIP => "227.3.1.10";
        public static string RemoteIp => "227.2.1.10";

        public static DeviceSetup DeviceSetupFactory(UeiDeviceInfo ueiDevice)
        {
            DeviceSetup result = null;
            if (null == ueiDevice.DeviceName)
                throw new ArgumentNullException("device name");

            switch (ueiDevice.DeviceName)
            {
                case DeviceMap2.AO308Literal:
                    result = new AO308Setup(new EndPoint(LocalIP, portNumber++), ueiDevice);
                    break;
                case DeviceMap2.DIO403Literal:
                    result = new DIO403Setup(new EndPoint(LocalIP, portNumber++), new EndPoint(RemoteIp, portNumber++), ueiDevice);
                    break;
                case DeviceMap2.DIO470Literal:
                    result = new DIO470Setup(new EndPoint(LocalIP, portNumber++), ueiDevice);
                    break;
                case DeviceMap2.AI201Literal:
                    result = new AI201100Setup(new EndPoint(RemoteIp, portNumber++), ueiDevice);
                    break;
                case DeviceMap2.SL508Literal:
                    var sl508 = new SL508892Setup(new EndPoint(LocalIP, portNumber++), new EndPoint(RemoteIp, portNumber++), ueiDevice);
                    foreach (var ch in sl508.Channels)
                    {
                        ch.LocalUdpPort = portNumber++;
                    }
                    result = sl508;
                    break;
                case DeviceMap2.SimuAO16Literal:
                    result = new SimuAO16Setup(new EndPoint(LocalIP, portNumber++), ueiDevice);
                    break;
                case DeviceMap2.AO322Literal:
                    result = new AO332Setup(new EndPoint(LocalIP, portNumber++), ueiDevice);
                    break;
                default:
                    Console.WriteLine($"Config: Device {ueiDevice.DeviceName} not supported.");
                    result = new DeviceSetup(null, null, ueiDevice);
                    break;
            }

            return result;
        }
    }

    [XmlInclude(typeof(AO308Setup))]
    [XmlInclude(typeof(DIO403Setup))]
    [XmlInclude(typeof(DIO470Setup))]
    [XmlInclude(typeof(AI201100Setup))]
    [XmlInclude(typeof(SL508892Setup))]
    [XmlInclude(typeof(AO332Setup))]
    [XmlInclude(typeof(BlockSensorSetup))]
    [XmlInclude(typeof(SimuAO16Setup))]
    public class CubeSetup : IEquatable<CubeSetup>
    {
        [XmlAttribute("Url")]
        public string CubeUrl { get; set; }
        //[XmlIgnore]
        //public int CubeId 
        //{ 
        //    get; 
        //    private set; 
        //}  = 0;
        public List<DeviceSetup> DeviceSetupList = new List<DeviceSetup>();

        //private List<Device> _ueiDeviceList;

        public CubeSetup()
        {
        }

        public CubeSetup(List<UeiDeviceInfo> deviceList, string cubeUrl)
        {
            CubeUrl = cubeUrl;

            foreach (var dev in deviceList)
            {
                if (null == dev)
                    throw new ArgumentNullException("null device");

                var dv = ConfigFactory.DeviceSetupFactory(dev);
                if (null != dv)
                {
                    DeviceSetupList.Add(dv);
                }
            }
            BlockSensorSetup bsSetup = BuildBlockSensorSetup( DeviceSetupList);
            if (null != bsSetup)
            {
                DeviceSetupList.Add(bsSetup);
            }
        }


        public bool Equals(CubeSetup other)
        {
            bool f1 = this.CubeUrl.Equals(other.CubeUrl);
            bool f2 = this.DeviceSetupList.SequenceEqual(other.DeviceSetupList);
            return f1 && f2;
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
            UeiDaq.DeviceCollection devColl = new UeiDaq.DeviceCollection(cubeUrl); // cubesetup should not depend on ueidaq.
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
        private BlockSensorSetup BuildBlockSensorSetup(List<DeviceSetup> deviceSetupList)
        {
            DeviceSetup dsa = deviceSetupList.Where(t => t.DeviceName == DeviceMap2.AO308Literal).Select(t => t).FirstOrDefault();
            DeviceSetup dsd = deviceSetupList.Where(t => t.DeviceName == DeviceMap2.DIO403Literal).Select(t => t).FirstOrDefault();

            if ((dsa != null) && (dsd != null))
            {
                BlockSensorSetup Blocksensor = new BlockSensorSetup(new EndPoint(ConfigFactory.LocalIP, 50105), "BlockSensor");

                Blocksensor.AnalogCardSlot = dsa.SlotNumber;
                Blocksensor.DigitalCardSlot = dsd.SlotNumber;
                return Blocksensor;
            }
            else
            {
                return null;
            }
        }

    }

    
    
    
    public class Config2 : IEquatable<Config2>
    {

        public AppSetup AppSetup;
        public List<CubeSetup> CubeSetupList = new List<CubeSetup>();

        public static string DafaultSettingsFilename => "UeiSettings2.config";
        public static string SettingsFilename { get; private set; } = DafaultSettingsFilename;

        public Config2() { }// this is for serialization. 
        public Config2(List<CubeSetup> cubeSetups)
        {
            AppSetup = new AppSetup();
            this.CubeSetupList.AddRange(cubeSetups);
        }

        /// <summary>
        /// Load config from file
        /// </summary>
        /// <returns></returns>
        public static Config2 LoadConfigFromFile(FileInfo configFile)
        {
            var serializer = new XmlSerializer(typeof(Config2));
            Config2 resultConfig = null;

            using (StreamReader sr = configFile.OpenText())
            {
                resultConfig = serializer.Deserialize(sr) as Config2;
                if (null != resultConfig)
                {
                    foreach (CubeSetup cSetup in resultConfig.CubeSetupList)
                    {
                        foreach (DeviceSetup dSetup in cSetup.DeviceSetupList)
                        {
                            dSetup.CubeUrl = cSetup.CubeUrl;
                        }
                    }
                }
            }
            return resultConfig;
        }
        public static Config2 BuildDefaultConfig(List<string> cubeUrlList)
        {
            List<CubeSetup> csetupList = new List<CubeSetup>();
            foreach (var url in cubeUrlList)
            {
                UeiDaq.DeviceCollection devColl = new UeiDaq.DeviceCollection(url);
                List<UeiDeviceInfo> rl = new List<UeiDeviceInfo>();
                foreach (UeiDaq.Device dev in devColl)
                {
                    if (dev == null)
                        continue; // this for the last entry, which is null
                    rl.Add(new UeiDeviceInfo(url, dev.GetIndex(), dev.GetDeviceName() ));
                }
                csetupList.Add(new CubeSetup(rl, url));
            }

            Config2 res = new Config2(csetupList);
            return res;

        }

        public DeviceSetup GetDeviceSetupEntry(string cubeUrl, int slotNumber)
        {
            if (this.CubeSetupList == null)
            {
                return null;
            }
            var cube = this.CubeSetupList.Where(e => e.CubeUrl == cubeUrl);
            var selectedCube = cube.FirstOrDefault();
            if (null == selectedCube)
            {
                return null;
            }
            var theSetups = selectedCube.DeviceSetupList.Where(d => d.SlotNumber == slotNumber);
            DeviceSetup result = theSetups.FirstOrDefault();
            return result;
        }
        public void SaveAs(FileInfo fileToSave, bool overwrite)
        {
            string fn = fileToSave.FullName;
            if (fileToSave.Exists)
            {
                if (overwrite)
                {
                    fileToSave.Delete();
                }
                else
                {
                    return;
                }
            }
            FileInfo newfile = new FileInfo(fn);
            var serializer = new XmlSerializer(typeof(Config2));
            using (var writer = newfile.OpenWrite()) // new StreamWriter(filename))
            {
                serializer.Serialize(writer, this);
            }
        }

        public bool Equals(Config2 other)
        {
            return CubeSetupList.SequenceEqual(other.CubeSetupList);
        }

        public static System.Net.IPAddress CubeUriToIpAddress(string url)
        {
            System.Net.IPAddress result = null;
            UriHostNameType ht = Uri.CheckHostName(url);
            //bool ok1 = ;
            if (Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                Uri u1 = new Uri(url);

                bool ok = System.Net.IPAddress.TryParse(u1.Host, out result);
                return (ok) ? result : null;
            }
            else
            {
                return null;
            }
        }

    }
    //public class ValidValuesClass
    //{
    //    public string ValidSerialModes;
    //    public string ValidBaudRates;
    //    public string ValidStopBitsValues;
    //    public string ValidParityValues;

    //    public ValidValuesClass()
    //    {
    //        ValidSerialModes = UeiBridge.Library.StaticMethods.GetEnumValues<UeiDaq.SerialPortMode>();
    //        ValidBaudRates = UeiBridge.Library.StaticMethods.GetEnumValues<UeiDaq.SerialPortSpeed>();
    //        ValidStopBitsValues = UeiBridge.Library.StaticMethods.GetEnumValues<UeiDaq.SerialPortStopBits>();
    //        ValidParityValues = UeiBridge.Library.StaticMethods.GetEnumValues<UeiDaq.SerialPortParity>();
    //    }
    //}
}
