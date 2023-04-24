using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UeiBridge.Library
{
    public static class DeviceMap
    {
        static Dictionary<int, string> _cardIdMap = new Dictionary<int, string>(); // card-id vs card name
        static DeviceMap()
        {
            _cardIdMap.Add(0, "AO-308");
            _cardIdMap.Add(4, "DIO-403");
            _cardIdMap.Add(6, "DIO-470");
            _cardIdMap.Add(1, "AI-201-100");
            _cardIdMap.Add(5, "SL-508-892");
            _cardIdMap.Add(32, "BlockSensor");
        }

        public static int GetCardIdFromCardName(string deviceName)
        {
            try
            {
                var p = _cardIdMap.ToList().Single(pair => pair.Value == deviceName);
                return p.Key;
            }
            catch (System.InvalidOperationException)
            {
                return -1;
            }
        }
    }

    public class DeviceItem
    {
        public readonly string DeviceName;
        public readonly int IcdIndex;
        public readonly string DeviceDesc;

        public DeviceItem(int icdIndex, string deviceName, string deviceDesc)
        {
            this.DeviceName = deviceName;
            this.IcdIndex = icdIndex;
            this.DeviceDesc = deviceDesc;
        }
    }
    public static class DeviceMap2
    {
        //public readonly static DeviceItem AO308;
        //public readonly static DeviceItem DIO403;
        //public readonly static DeviceItem DIO470;
        //public readonly static DeviceItem AI201;
        //public readonly static DeviceItem SL508;
        //public readonly static DeviceItem Blocksensor;

        public const string AO308Literal = "AO-308";
        public const string DIO403Literal = "DIO-403";
        public const string DIO470Literal = "DIO-470";
        public const string AI201Literal = "AI-201-100";
        public const string SL508Literal = "SL-508-892";
        public const string BlocksensorLiteral = "BlockSensor";
        public const string AO16Literal = "Simu-AO16";

        static List<DeviceItem> _deviceItemList = new List<DeviceItem>();
        static DeviceMap2()
        {
            //AO308 = new DeviceItem(0, "AO-308", "Analog output");
            //DIO403 = new DeviceItem(4, "DIO-403", "Digital Input/Output");
            //DIO470 = new DeviceItem(6, "DIO-470", "Electro-Mechanical relay");
            //AI201 = new DeviceItem(1, "AI-201-100", "Analog input");
            //SL508 = new DeviceItem(5, "SL-508-892", "RS-422/485 Serial Port");
            //Blocksensor = new DeviceItem(32, "BlockSensor", "Block sensor (virtual)");

            _deviceItemList.Add(new DeviceItem(0, AO308Literal, "Analog output"));
            _deviceItemList.Add(new DeviceItem(4, DIO403Literal, "Digital Input/Output"));
            _deviceItemList.Add(new DeviceItem(6, DIO470Literal, "Electro-Mechanical relay"));
            _deviceItemList.Add(new DeviceItem(1, AI201Literal, "Analog input"));
            _deviceItemList.Add(new DeviceItem(5, SL508Literal, "RS-232/422/485 Serial Port"));
            _deviceItemList.Add(new DeviceItem(32, BlocksensorLiteral, "Block sensor (virtual)"));

            //_deviceItemList.Add(AO308);
            //_deviceItemList.Add(DIO403);
            //_deviceItemList.Add(DIO470);
            //_deviceItemList.Add(AI201);
            //_deviceItemList.Add(SL508);
            //_deviceItemList.Add(Blocksensor);
        }

        public static string GetDeviceDesc(string deviceName)
        {
            var x = _deviceItemList.Where(n => n.DeviceName == deviceName).Select(n => n.DeviceDesc);
            string s = x.FirstOrDefault();
            return s;
        }

        public static int GetCardIdFromCardName(string deviceName)
        {
            try
            {
                var p = _deviceItemList.Where(i => i.DeviceName == deviceName).Select(i => i.IcdIndex);
                if (p.Count() != 1)
                {
                    throw new ArgumentException();
                }
                return p.FirstOrDefault();
            }
            catch (System.InvalidOperationException)
            {
                return -1;
            }
        }

    }
}
