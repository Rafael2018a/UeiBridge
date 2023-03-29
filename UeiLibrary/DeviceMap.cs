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
}
