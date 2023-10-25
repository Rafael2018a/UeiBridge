using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.IO;
using System.Net;
using UeiDaq;
using UeiBridge.CubeSetupTypes;

/// <summary>
/// All classes in this file MUST NOT depend on any other module in the project
/// </summary>
namespace UeiBridge.Library
{
    /// <summary>
    /// This serializable class represents ip address+port pair.
    /// </summary>
    public class EndPoint : IEquatable<EndPoint>
    {
        public string Address { get;  set; }
        public int Port { get;  set; }
        public EndPoint()
        {
        }
        public EndPoint(string addressString, int port)
        {
            Address = addressString;
            Port = port;
        }
        public void SetAddress(string add)
        {
            IPAddress ip;
            bool ok = IPAddress.TryParse(Address, out ip);
            if (ok)
            {
                Address = add;
            }
        }
        public void SetPort(int port)
        {
            if (port>=0)
            {
                this.Port = port;
            }
        }
        public bool Equals(EndPoint other)
        {
            return (this.Address == other.Address) && (this.Port == other.Port);
        }
        public IPEndPoint ToIpEp() 
        {
            IPAddress ip;
            bool ok = IPAddress.TryParse(Address, out ip);
            if (ok)
            {
                IPEndPoint ipep = new IPEndPoint(ip, Port);
                return ipep;
            }
            else
            {
                return null;
            }
        }
        public static EndPoint MakeEndPoint(string addressString, int port)
        {
            EndPoint ep = new EndPoint(addressString, port);

            if (null != ep.ToIpEp())
            {
                return ep;
            }
            else
            {
                return null;
            }
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
    public class CANChannelSetup
    {
        public CANChannelSetup()
        {
        }

        public CANChannelSetup(int channelIndex)
        {
            this.ChannelIndex = channelIndex;
        }

        public int ChannelIndex { set; get; }
        public CANPortSpeed Speed { get; set; } = CANPortSpeed.BitsPerSecond100K;
        public CANFrameFormat FrameFormat { get; set; } = CANFrameFormat.Extended;
        public CANPortMode PortMode { get; set; } = CANPortMode.Normal;
    }
    public class AppSetup
    {
        public string SelectedNicForMulticast { get; private set; } = "221.109.251.103";
        public EndPoint StatusViewerEP = new EndPoint("239.10.10.17", 5093);

        public AppSetup()
        {
            SelectedNicForMulticast = System.Configuration.ConfigurationManager.AppSettings["SelectedNicForMulticast"];
        }


    }

    public class DIOChannel
    {
        [XmlAttribute()]
        public byte OctetIndex { get; set; }
        public MessageWay Way { get; set; }
        public DIOChannel(byte octetNumber, MessageWay way)
        {
            OctetIndex = octetNumber;
            Way = way;
        }
        public DIOChannel() { }
    }


    public class ConfigFactory
    {
        int _portNumber = 50035;
        public static string LocalIP => "227.3.1.10";
        public static string RemoteIp => "227.2.1.10";
        public const int PortNumberStart = 50000;
        public const int DIO403NumberOfChannels = 6;
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
                case DeviceMap2.AO322Literal:
                    result = new AO308Setup(new EndPoint(LocalIP, _portNumber++), ueiDevice);
                    break;
                case DeviceMap2.DIO403Literal:
                    result = new DIO403Setup(new EndPoint(LocalIP, _portNumber++), new EndPoint(RemoteIp, _portNumber++), ueiDevice, DIO403NumberOfChannels);
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
                case DeviceMap2.CAN503Literal:
                    result = new CAN503Setup(new EndPoint(LocalIP, _portNumber++), new EndPoint(RemoteIp, _portNumber++), ueiDevice);
                    break;
                default:
                    Console.WriteLine($"Config: Device {ueiDevice.DeviceName} not supported.");
                    result = new DeviceSetup(null, null, ueiDevice);
                    break;
            }

            return result;
        }
    }

    public class Config2 : IEquatable<Config2>
    {
        [XmlIgnore]
        public AppSetup AppSetup;
        public List<CubeSetup> CubeSetupList = new List<CubeSetup>();

        public static string DefaultSettingsFilename => "Cube2.config"; // to be removed
        public static string SettingsFilename { get; private set; } = DefaultSettingsFilename;

        public Config2(List<string> cubeUrlList)
        { 
            AppSetup = LoadGeneralSetup();
            this.CubeSetupList = GetSetupForConnectedCubes(cubeUrlList);
        }
        public Config2() { } // empty c-tor must exist for serialization.
        private Config2(List<CubeSetup> cubeSetups) // for default config
        {
            AppSetup = new AppSetup();
            this.CubeSetupList.AddRange(cubeSetups);
        }

        /// <summary>
        /// Load config from file
        /// </summary>
        /// <returns></returns>
        [Obsolete]
        public static Config2 LoadConfigFromFile_notInUse(FileInfo configFile)
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
                    string filename = (cube_id == UeiDeviceInfo.SimuCubeId) ? "Cube.simu.config" : $"Cube{cube_id}.config";
                    CubeSetup csFromFile = CubeSetup.LoadCubeSetupFromFile( new FileInfo( filename));
                    if (null == csFromFile) // if failed to load from file
                    {
                        CubeSetup defaultSetup = new CubeSetup(devInfoList); // create default
                        resultConfig.CubeSetupList.Add(defaultSetup);

                        // save default to file
                        using (FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write))
                        {
                            var serializer = new XmlSerializer(typeof(CubeSetup));
                            serializer.Serialize(fs, defaultSetup);
                            Console.WriteLine($"Config: File {filename} created");
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

        public static CubeSetup LoadCubeSetupFromFile_moved(FileInfo filename)
        {
            var serializer = new XmlSerializer(typeof(CubeSetup));
            CubeSetup resultSetup = null;
            FileInfo configFile = filename;// = new FileInfo(filename);

            if (configFile.Exists)
            {
                using (StreamReader sr = configFile.OpenText())
                {
                    resultSetup = serializer.Deserialize(sr) as CubeSetup;
                    resultSetup.AssociatedFileFullname = configFile.FullName;
                }
            }
            return resultSetup;
        }

        private static AppSetup LoadGeneralSetup()
        {
            AppSetup resultSetup = new AppSetup();
            //resultSetup.SelectedNicForMCast = "221.109.251.103";
            resultSetup.StatusViewerEP = new EndPoint("239.10.10.17", 5093);
            return resultSetup;
        }

        public void SavePerCube(string basefilename, bool overwrite)
        {
            foreach (CubeSetup cstp in this.CubeSetupList)
            {
                int cubeid = StaticMethods.GetCubeId(cstp.CubeUrl);
                FileInfo newfile = new FileInfo($"{basefilename}.cube{cubeid}.config");
                var serializer = new XmlSerializer(typeof(CubeSetup));
                using (var writer = newfile.OpenWrite()) // new StreamWriter(filename))
                {
                    serializer.Serialize(writer, cstp);
                }
            }
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
        public DeviceSetup GetDeviceSetupEntry(string cubeUrl, string deviceName)
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
            var theSetups = selectedCube.DeviceSetupList.Where(d => d.DeviceName == deviceName);
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


        /// <summary>
        /// Result list shall contain entries only for connected cubes
        /// </summary>
        /// <param name="cubeUrlList"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static List<CubeSetup> GetSetupForConnectedCubes(List<string> cubeUrlList)
        {
            List<CubeSetup> cubeSetupList = new List<CubeSetup>();
            // load settings per cube
            foreach (string cubeurl in cubeUrlList)
            {
                UeiDaq.DeviceCollection devColl = new UeiDaq.DeviceCollection(cubeurl);
                List<UeiDeviceInfo> devInfoList = StaticMethods.DeviceCollectionToDeviceInfoList(devColl, cubeurl);

                // if cube connected, load/create setup
                if (null != devInfoList && devInfoList.Count > 0)
                {
                    int cube_id = devInfoList[0].CubeId;
                    string filename = CubeSetup.GetSelfFilename(cube_id); //(cube_id == UeiDeviceInfo.SimuCubeId) ? "Cube.simu.config" : $"Cube{cube_id}.config";
                    CubeSetup cs = CubeSetup.LoadCubeSetupFromFile( new FileInfo( filename));
                    if (null == cs) // if failed to load from file
                    {
                        cs = new CubeSetup(devInfoList); // create default
                        cs.AssociatedFileFullname = (new FileInfo(filename)).FullName;
                    }
                    cubeSetupList.Add(cs);
                }
            }
            return cubeSetupList;
        }
    }
}
