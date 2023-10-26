using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using System.IO;
using UeiBridge.Library;

/// <summary>
/// All classes in this file MUST NOT depend on any other module in the project
/// </summary>
namespace UeiBridge.CubeSetupTypes
{


    [XmlInclude(typeof(AO308Setup))]
    [XmlInclude(typeof(DIO403Setup))]
    [XmlInclude(typeof(DIO470Setup))]
    [XmlInclude(typeof(AI201100Setup))]
    [XmlInclude(typeof(SL508892Setup))]
    //[XmlInclude(typeof(AO332Setup))]
    [XmlInclude(typeof(BlockSensorSetup))]
    [XmlInclude(typeof(SimuAO16Setup))]
    [XmlInclude(typeof(CAN503Setup))]
    public class CubeSetup : IEquatable<CubeSetup>
    {
        public string CubeUrl { get; set; } // must be public for the  serializer
        public string TypeNickname { get; set; }
        public string TypeDesc { get; set; }
        public int TypeId { get; set; }
        public List<DeviceSetup> DeviceSetupList { get; set; } // don't make private set
        //public string OriginFileFullName { get; set; } // file associated with current config
        //[XmlIgnore]
        //public string AssociatedFileFullname { get; set; }
        [XmlIgnore]
        public bool IsLoadedFromFile { get; private set; }
        public CubeSetup()
        {
            IsLoadedFromFile = true;
        }

        public static string GetSelfFilename(int cube_id)
        {
            string filename = cube_id == UeiDeviceInfo.SimuCubeId ? "Cube.simu.config" : $"Cube{cube_id}.config";
            return filename;
        }
        public static string GetSelfFilename(string nickname)
        {
            string filename = $"Cubesetup.{nickname}.config";
            return filename;
        }
        /// <summary>
        /// Build default cube setup
        /// </summary>
        /// <param name="deviceList"></param>
        public CubeSetup(List<UeiDeviceInfo> deviceList)
        {
            IsLoadedFromFile = false;

            CubeUrl = deviceList[0].CubeUrl;
            int cubeId = deviceList[0].CubeId;
            TypeNickname = $"Nick{cubeId}";
            TypeDesc = TypeNickname + " desc";
            TypeId = cubeId * 10;
            ConfigFactory cf = new ConfigFactory(ConfigFactory.PortNumberStart + cubeId * 100);

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
                //DeviceSetupList.Add(bsSetup);
            }
        }

        public bool Equals(CubeSetup other)
        {
            //bool f1 = this.CubeUrl.Equals(other.CubeUrl);
            try
            {
                bool f2 = DeviceSetupList.SequenceEqual(other.DeviceSetupList);
                return f2;
            }
            catch (Exception ex)
            {
                return false;
            }
            //return f2;
        }

        public int GetCubeId()
        {
            return StaticMethods.GetCubeId(CubeUrl);
        }
        private BlockSensorSetup BuildBlockSensorSetup(List<DeviceSetup> deviceSetupList)
        {
            DeviceSetup dsa = deviceSetupList.Where(t => t.DeviceName == DeviceMap2.AO308Literal).Select(t => t).FirstOrDefault();
            DeviceSetup dsd = deviceSetupList.Where(t => t.DeviceName == DeviceMap2.DIO403Literal).Select(t => t).FirstOrDefault();

            if (dsa != null && dsd != null)
            {
                //cf = new ConfigFactory();
                UeiDeviceInfo info = new UeiDeviceInfo(dsa.CubeUrl, dsa.SlotNumber, DeviceMap2.BlocksensorLiteral);
                BlockSensorSetup Blocksensor = new BlockSensorSetup(new EndPoint(ConfigFactory.LocalIP, 50105), info);

                //Blocksensor.AnalogCardSlot = dsa.SlotNumber;
                Blocksensor.DigitalCardSlot = dsd.SlotNumber;
                return Blocksensor;
            }
            else
            {
                return null;
            }
        }

        [Obsolete]
        public void Serialize1(string AssociatedFileFullname)
        {
            //string filename = GetSelfFilename( StaticMethods.GetCubeId(this.CubeUrl));

            if (File.Exists(AssociatedFileFullname))
            {
                File.Delete(AssociatedFileFullname);
            }

            FileInfo fi = new FileInfo(AssociatedFileFullname);

            using (FileStream fs = fi.OpenWrite())
            {
                var serializer = new XmlSerializer(GetType());
                serializer.Serialize(fs, this);
                Console.WriteLine($"Config: File {fi.Name} created");
            }
        }
        public T GetDeviceSetupEntry<T>(UeiDeviceInfo devInfo) where T : DeviceSetup
        {
            return GetDeviceSetupEntry(devInfo.DeviceSlot) as T;
        }
        public DeviceSetup GetDeviceSetupEntry(int slotNumber)
        {
            var selectedCube = this;
            if (null == selectedCube)
            {
                return null;
            }
            var theSetups = selectedCube.DeviceSetupList.Where(d => d.SlotNumber == slotNumber);
            DeviceSetup devSetup = theSetups.FirstOrDefault();
            if (null != devSetup)
            {
                devSetup.CubeUrl = CubeUrl;
            }
            return devSetup;
        }

        [Obsolete]
        public static CubeSetup LoadCubeSetupFromFile1(FileInfo filename)
        {
            var serializer = new XmlSerializer(typeof(CubeSetup));
            CubeSetup resultSetup = null;
            FileInfo configFile = filename;// = new FileInfo(filename);

            if (configFile.Exists)
            {
                using (StreamReader sr = configFile.OpenText())
                {
                    resultSetup = serializer.Deserialize(sr) as CubeSetup;
                    ///resultSetup.AssociatedFileFullname = configFile.FullName;
                }
            }
            return resultSetup;
        }

    }
}
