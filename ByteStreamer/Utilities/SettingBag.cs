using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ByteStreamer.Utilities
{
    [Serializable]
    class SettingBag
    {
        public string destinationIp = "92.220.113.22";
        public int destinationPort = 5000;
        public double ratePercent = 0;
        public int blockLength = 0;
        public int waitStatesMS = 1;
        //public double playRateMbps;
    }
}
