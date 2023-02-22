using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ByteStreamer3.Utilities
{
    class ProjectTypes
    {
    }
    //public class PlayItem1
    //{
    //    public string Name { get; set; }

    //    public int PlayedBlocks { get; set; }
    //    public string Mail { get; set; }
    //    //public override string ToString()
    //    //{
    //    //    return this.Name + ", " + this.Age + " years old";
    //    //}
    //}
    [Serializable]
    class SettingBag
    {
        public string PlayFolder=".";
        //public string destinationIp = "92.220.113.22";
        //public int destinationPort = 5000;
        //public double ratePercent = 0;
        //public int blockLength = 0;
        //public int waitStatesMS = 1;
        //public double playRateMbps;
    }


}
