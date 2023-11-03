using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.IO;
using UeiDaq;
using UeiBridge.CubeSetupTypes;


/// <summary>
/// All classes in this file MUST NOT depend on any other module in the project
/// </summary>
namespace UeiBridge.Library
{
    public class AppSetup // tbd. not belongs here
    {
        public string SelectedNicForMulticast { get; private set; } = "221.109.251.103";
        public EndPoint StatusViewerEP = new EndPoint("239.10.10.17", 5093);

        public AppSetup()
        {
            SelectedNicForMulticast = System.Configuration.ConfigurationManager.AppSettings["SelectedNicForMulticast"];
        }
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
            this.CubeSetupList = GetSetupForCubes(cubeUrlList);
        }
        public Config2() { } // empty c-tor must exist for serialization.
        private Config2(List<CubeSetup> cubeSetups) // for default config
        {
            AppSetup = new AppSetup();
            this.CubeSetupList.AddRange(cubeSetups);
        }

        private static AppSetup LoadGeneralSetup()
        {
            AppSetup resultSetup = new AppSetup();
            //resultSetup.SelectedNicForMCast = "221.109.251.103";
            resultSetup.StatusViewerEP = new EndPoint("239.10.10.17", 5093);
            return resultSetup;
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
        [Obsolete]
        public void SaveAs1(FileInfo fileToSave, bool overwrite)
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


        public static List<CubeSetup> GetSetupForCubes(List<string> cubeUrlList) 
        {
            List<CubeSetup> cubeSetupList = new List<CubeSetup>();
            // load settings per cube
            foreach (string cubeurl in cubeUrlList)
            {
                UeiDaq.DeviceCollection devCollection = new UeiDaq.DeviceCollection(cubeurl);
                List<UeiDeviceInfo> devInfoList = StaticMethods.DeviceCollectionToDeviceInfoList(devCollection, cubeurl);

                // if cube connected, load/create setup
                if (null != devInfoList && devInfoList.Count > 0)
                {
                    string filename = CubeSetup.GetSelfFilename(devInfoList[0].CubeId); //(cube_id == UeiDeviceInfo.SimuCubeId) ? "Cube.simu.config" : $"Cube{cube_id}.config";
                    CubeSetupLoader csl = new CubeSetupLoader( new FileInfo(filename));

                    if (null == csl.CubeSetupMain) // if failed to load from file
                    {
                        CubeSetup cs = new CubeSetup(devInfoList); // create default
                        cubeSetupList.Add(cs);
                    }
                    else
                    {
                        cubeSetupList.Add(csl.CubeSetupMain);
                    }
                    
                }
            }
            return cubeSetupList;
        }
    }
}
