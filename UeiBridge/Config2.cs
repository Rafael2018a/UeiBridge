using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.IO;
using UeiDaq;

namespace UeiBridge
{
    public class DeviceSetup
    {
        public int SlotNumber;
        public string DeviceName;
        public EndPoint LocalEndPoint;
        public EndPoint DestEndPoint;
        
        public DeviceSetup(EndPoint localEndPoint, EndPoint destEndPoint, UeiDaq.Device device)
        {
            LocalEndPoint = localEndPoint;
            DestEndPoint = destEndPoint;
            DeviceName = device.GetDeviceName();
            SlotNumber = device.GetIndex();
        }
        protected DeviceSetup()
        {
        }
        private string cubeUrl;
        public string CubeUrl { get => cubeUrl; set => cubeUrl = value; }
    }
    public class AO308Setup : DeviceSetup
    {
        public AO308Setup()
        {
        }
        public AO308Setup(EndPoint localEndPoint, UeiDaq.Device device) : base(localEndPoint, null, device)
        {
        }
    }
    public class AI201100Setup : DeviceSetup
    {

    }
    public class DIO430Setup : DeviceSetup
    {
        //public Direction[] BitOctets;
        public DIO430Setup()
        {
        }

        public DIO430Setup(EndPoint localEndPoint, EndPoint destEndPoint, UeiDaq.Device device) : base(localEndPoint, destEndPoint, device)
        {
            //BitOctets = new Direction[4];
            //BitOctets[0] = Direction.input;
            //BitOctets[1] = Direction.input;
            //BitOctets[2] = Direction.output;
            //BitOctets[3] = Direction.output;
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
                Channels[ch] = new SerialChannel($"Com{ch}");
            }
        }
    }

    public static class ConfigFactory
    {
        public static DeviceSetup CreateConfigInstance( Device ueiDevice)
        {
            DeviceSetup result=null;
            
            switch (ueiDevice.GetDeviceName())
            {
                case "AO-308":
                    result = new AO308Setup( new EndPoint("227.3.1.10", 50035), ueiDevice);
                    break;
                case "DIO-430":
                    result = new DIO430Setup(new EndPoint("227.3.1.10", 50036), new EndPoint("227.2.1.10", 51036), ueiDevice); 
                    break;
                case "AI-210-100":
                    result = new AI201100Setup();
                    break;
                case "SL-508-892":
                    result = new SL508892Setup(new EndPoint("227.3.1.10", 50036), new EndPoint("227.2.1.10", 51036), ueiDevice);
                    break;
                default:
                    Console.WriteLine($"Missing setup-class for device {ueiDevice.GetDeviceName()}");
                    result = new DeviceSetup(null, null, ueiDevice);
                    break;
            }

            return result;
        }
    }

    public class CubeSetup
    {
        //const int tbd = 7;
        public string CubeUrl;
        public int CubeNumber;
        public DeviceSetup[] SlotList;

        List<Device> _deviceList;

        public CubeSetup()
        {
        }
        public CubeSetup(string cubeUrl, int cubeNumber)
        {
            CubeUrl = cubeUrl;
            CubeNumber = cubeNumber;
            _deviceList = StaticMethods.GetDeviceList();
            SlotList = new DeviceSetup[_deviceList.Count];
            foreach (Device dev in _deviceList)
            {
                SlotList[dev.GetIndex()] = ConfigFactory.CreateConfigInstance(dev);
            }
        }
    }
    [XmlInclude(typeof(AO308Setup))]
    [XmlInclude(typeof(DIO430Setup))]
    [XmlInclude(typeof(AI201100Setup))]
    [XmlInclude(typeof(SL508892Setup))]
    public class Config2
    {
        private static Config2 _instance;
        private static object lockObject = new object();

        public string[] CubeUrlList = new string[1];
        public CubeSetup[] UeiCubes = new CubeSetup[1];

        private Config2()
        {
            CubeUrlList[0] = "pdna://192.168.100.2/";
            for(int i=0; i< UeiCubes.Length; i++)
            {
                UeiCubes[i] = new CubeSetup("pdna://192.168.100.2/", i);
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
        internal static Config2 LoadConfig()
        {
            string filename = "UeiSettings2.config";
            var serializer = new XmlSerializer(typeof(Config2));
            Config2 resultConfig = null;
            if (System.IO.File.Exists(filename))
            {
                try
                {
                    using (StreamReader sr = File.OpenText(filename))
                    {
                        resultConfig = serializer.Deserialize(sr) as Config2;
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
                // make fresh config and write it to file
                resultConfig = new Config2();
                using (var writer = new StreamWriter(filename))
                {
                    serializer.Serialize(writer, resultConfig);
                    Console.WriteLine($"New default settings file created. {filename}");
                }
            }

            return resultConfig;
        }
    }
}
