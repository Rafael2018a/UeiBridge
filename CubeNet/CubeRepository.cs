using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace UeiBridge.CubeNet
{
    class CubeType
    {
        public string CubeSignature { get; set; }
        public string NickName { get; set; }
        public int TypeId { get; set; }
        public string Desc { get; set; }
        //public List<System.Net.IPAddress> PertainCubeList { get; private set; }
        public List<string> PertainCubeList { get; set; }

        public CubeType(string nickName, string desc)
        {
            NickName = nickName;
            Desc = desc;
            PertainCubeList = new List<string>();
        }
        public void AddCube( System.Net.IPAddress cubeIp)
        {
            PertainCubeList.Add(cubeIp.ToString());
        }

        internal void SetSignature(string cubeSignature)
        {
            CubeSignature = cubeSignature;
        }
    }
    class CubeRepository
    {
        public List<CubeType> CubeTypeList { get; set; }
        public CubeRepository()
        {
            CubeTypeList = new List<CubeType>();
        }
    }

//    SlotMap: “DIO403- SL508- SL508-  SL508- SL508- DIO452”
//NickName: “mcc”
//TypeId: 1
//“Desc”: “this cube should connect to mcc”
//CubeList: [“192.168.100,3”, “192.168.100,15”, “192.168.100,17”]
}


