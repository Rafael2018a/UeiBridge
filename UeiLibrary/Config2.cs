using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.IO;
using System.Net;

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
        [XmlElement(ElementName = "DeviceSlot")]
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
    public class DIOChannel
    {
        [XmlAttribute()]
        public byte OctetIndex { get; set; }
        public MessageWay Way { get; set; }
        public DIOChannel( byte octetNumber, MessageWay way)
        {
            OctetIndex = octetNumber;
            Way = way;
        }
        public DIOChannel() {}
    }
    public class DIO403Setup : DeviceSetup
    {
        public List<DIOChannel> IOChannelList { get; set; }
        //public Direction[] BitOctets;
        public DIO403Setup()
        {
        }

        public DIO403Setup(EndPoint localEndPoint, EndPoint destEndPoint, UeiDeviceInfo device) : base(localEndPoint, destEndPoint, device)
        {
            IOChannelList = new List<DIOChannel>();
            for (byte ch = 0; ch < 6; ch++)
            {
                MessageWay w = (ch % 2 == 0) ? MessageWay.upstream : MessageWay.downstream;
                IOChannelList.Add(new DIOChannel(ch, w));
            }
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
                Channels.Add(new SerialChannelSetup(chIndex, UeiDaq.SerialPortSpeed.BitsPerSecond19200));
            }
        }
    }

    public class ConfigFactory
    {
        int _portNumber = 50035;
        public static string LocalIP => "227.3.1.10";
        public static string RemoteIp => "227.2.1.10";
        public const int PortNumberStart = 50000;
        public ConfigFactory(int initialPortNumber)
        {
            _portNumber = initialPortNumber;
        }

        public DeviceSetup BuildDefaultSetup(UeiDeviceInfo ueiDevice)
        {
            DeviceSetup result = null;
            if (null == ueiDevice.DeviceName)
                throw new ArgumentNullException("device name");

            switch (ueiDevice.DeviceName)
            {
                case DeviceMap2.AO308Literal:
                    result = new AO308Setup(new EndPoint(LocalIP, _portNumber++), ueiDevice);
                    break;
                case DeviceMap2.DIO403Literal:
                    result = new DIO403Setup(new EndPoint(LocalIP, _portNumber++), new EndPoint(RemoteIp, _portNumber++), ueiDevice);
                    break;
                case DeviceMap2.DIO470Literal:
                    result = new DIO470Setup(new EndPoint(LocalIP, _portNumber++), ueiDevice);
                    break;
                case DeviceMap2.AI201Literal:
                    result = new AI201100Setup(new EndPoint(RemoteIp, _portNumber++), ueiDevice);
                    break;
                case DeviceMap2.SL508Literal:
                    var sl508 = new SL508892Setup(new EndPoint(LocalIP, _portNumber++), new EndPoint(RemoteIp, _portNumber++), ueiDevice);
                    foreach (var ch in sl508.Channels)
                    {
                        ch.LocalUdpPort = _portNumber++;
                    }
                    result = sl508;
                    break;
                case DeviceMap2.SimuAO16Literal:
                    result = new SimuAO16Setup(new EndPoint(LocalIP, _portNumber++), ueiDevice);
                    break;
                case DeviceMap2.AO322Literal:
                    result = new AO332Setup(new EndPoint(LocalIP, _portNumber++), ueiDevice);
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
        public string CubeUrl { get; set; } // must be public for the  serializer
        public List<DeviceSetup> DeviceSetupList { get; set; }

        public CubeSetup()
        {
        }

        /// <summary>
        /// Build default cube setup
        /// </summary>
        /// <param name="deviceList"></param>
        public CubeSetup(List<UeiDeviceInfo> deviceList)
        {
            CubeUrl = deviceList[0].CubeUrl;
            int cubeId = deviceList[0].CubeId;
            ConfigFactory cf = new ConfigFactory( ConfigFactory.PortNumberStart+cubeId*100 );


            DeviceSetupList = new List<DeviceSetup>();
            foreach (var dev in deviceList)
            {
                if (null == dev)
                {
                    continue;
                }

                DeviceSetup ds = cf.BuildDefaultSetup(dev);
                if (null != ds)
                {
                    DeviceSetupList.Add(ds);
                }
            }
            BlockSensorSetup bsSetup = BuildBlockSensorSetup(DeviceSetupList);
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
                 //cf = new ConfigFactory();
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
        private Config2(List<CubeSetup> cubeSetups)
        {
            AppSetup = new AppSetup();
            this.CubeSetupList.AddRange(cubeSetups);
        }

        /// <summary>
        /// Load config from file
        /// </summary>
        /// <returns></returns>
        [Obsolete]
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
        /// <summary>
        /// for each cube url, load unique config-file,
        /// if config-file not exists, use default. 
        /// save the default config as new config.
        /// </summary>
        /// <param name="cubeUrlList"></param>
        /// <returns></returns>
        public static Config2 LoadConfig(List<string> cubeUrlList)
        {
            // load global setting from app-config
            Config2 resultConfig = new Config2();
            resultConfig.AppSetup = LoadGeneralSetup();

            // load settings per cube
            foreach (string cubeurl in cubeUrlList)
            {
                UeiDaq.DeviceCollection devColl = new UeiDaq.DeviceCollection(cubeurl);
                List<UeiDeviceInfo> devInfoList = StaticMethods.DeviceCollectionToDeviceInfoList(devColl, cubeurl);

                // if cube connected, load/create setup
                if (null != devInfoList && devInfoList.Count > 0)
                {
                    int cube_id = devInfoList[0].CubeId;
                    string filename = (cube_id == -2) ? "Cube.simu.config" : $"Cube{cube_id}.config";
                    CubeSetup csFromFile = LoadCubeSetupFromFile(filename);
                    if (null==csFromFile) // if failed to load from file
                    {
                        CubeSetup defaultSetup = new CubeSetup(devInfoList); // create default
                        resultConfig.CubeSetupList.Add(defaultSetup);
                        
                        // save default to file
                        using (FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write))
                        {
                            var serializer = new XmlSerializer(typeof(CubeSetup));
                            serializer.Serialize(fs, defaultSetup);
                        }
                    }
                    else
                    {
                        resultConfig.CubeSetupList.Add(csFromFile);
                    }
                }
            }

            return resultConfig;
        }
        /// <summary>
        /// 1. get config from app config
        /// 2. load config from cubeX.config files
        /// 3. Build&Save config per missing cubeX.config 
        /// </summary>
        //public static CubeSetup GetCubeSetup(List<UeiDeviceInfo> deviceInfoList)
        //{
        //    CubeSetup resutlCubeSetup = null;
        //    //foreach (List<UeiDeviceInfo> cubeDeviceList in cubeListList)

        //    int cubeid = deviceInfoList[0].CubeId;
        //    string filename = (cubeid == -2) ? "Cube.simu.config" : $"Cube{cubeid}.config";
        //    CubeSetup cs = LoadCubeSetupFromFile(filename);

        //    if (null == cs) // if can't load from file
        //    {
        //        CubeSetup resutlSetup = new CubeSetup(deviceInfoList);

        //        using (FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write))
        //        {
        //            var serializer = new XmlSerializer(typeof(CubeSetup));
        //            serializer.Serialize(fs, resutlSetup);
        //        }

        //    }
        //    resutlCubeSetup = cs;

        //    return resutlCubeSetup;
        //}

        //private static CubeSetup BuildDefaultCubeSetup(List<UeiDeviceInfo> deviceList)
        //{
        //    CubeSetup resutlSetup = new CubeSetup(deviceList);

        //}

        public static CubeSetup LoadCubeSetupFromFile(string filename)
        {
            var serializer = new XmlSerializer(typeof(CubeSetup));
            CubeSetup resultConfig = null;
            FileInfo configFile = new FileInfo(filename);

            if (configFile.Exists)
            {
                using (StreamReader sr = configFile.OpenText())
                {
                    resultConfig = serializer.Deserialize(sr) as CubeSetup;
                }
            }
            return resultConfig;
        }

        private static AppSetup LoadGeneralSetup()
        {
            AppSetup resultSetup = new AppSetup();
            resultSetup.SelectedNicForMCast = "221.109.251.103";
            resultSetup.StatusViewerEP = new EndPoint("239.10.10.17", 5093);
            return resultSetup;
        }

        public void SavePerCube(string basefilename, bool overwrite)
        {
            foreach (CubeSetup cstp in this.CubeSetupList)
            {
                int cubeid = CubeUrlToCubeId(cstp.CubeUrl);
                FileInfo newfile = new FileInfo($"{basefilename}.cube{cubeid}.config");
                var serializer = new XmlSerializer(typeof(CubeSetup));
                using (var writer = newfile.OpenWrite()) // new StreamWriter(filename))
                {
                    serializer.Serialize(writer, cstp);
                }
            }
        }
        public static int CubeUrlToCubeId(string url)
        {
            System.Net.IPAddress ip = CubeUriToIpAddress(url);
            return (null == ip) ? -1 : ip.GetAddressBytes()[3];
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
                    rl.Add(new UeiDeviceInfo(url, dev.GetIndex(), dev.GetDeviceName()));
                }
                csetupList.Add(new CubeSetup(rl));
            }

            Config2 res = new Config2(csetupList);
            return res;

        }

        public T GetDeviceSetupEntry<T>(UeiDeviceInfo devInfo) where T : DeviceSetup
        {
            return GetDeviceSetupEntry(devInfo.CubeUrl, devInfo.DeviceSlot) as T;
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
            if (null != result)
            {
                result.CubeUrl = cubeUrl;
            }
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

            try
            {
                Uri u1 = new Uri(url);
                result = System.Net.IPAddress.Parse(u1.Host);
            }
            catch (Exception)
            {
            }
            return result;
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
