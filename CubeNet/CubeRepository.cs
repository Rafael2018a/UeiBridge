// Ignore Spelling: Uei

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace UeiBridge.CubeNet
{
    public class CubeRepository: IEquatable<CubeRepository>
    {
        public List<CubeType> CubeTypeList { get; set; }
        public CubeRepository()
        {
            CubeTypeList = new List<CubeType>();
        }

        public bool Equals(CubeRepository other)
        {
            //return this.CubeTypeList[0].Equals(other.CubeTypeList[0]);
            return this.CubeTypeList.SequenceEqual(other.CubeTypeList);
        }
    }

//    SlotMap: “DIO403- SL508- SL508-  SL508- SL508- DIO452”
//NickName: “mcc”
//TypeId: 1
//“Desc”: “this cube should connect to mcc”
//CubeList: [“192.168.100,3”, “192.168.100,15”, “192.168.100,17”]
}


