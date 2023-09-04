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
        public string SlotSignature { get; private set; }
        public string NickName { get; private set; }
        public int TypeId { get; private set; }
        public string Desc { get; private set; }
        public List<System.Net.IPAddress> PertainCubeList { get; private set; }

        public CubeType(string nickName, string desc)
        {
            NickName = nickName;
            Desc = desc;
            PertainCubeList = new List<IPAddress>();
        }
        public void AddCube( System.Net.IPAddress cubeIp)
        {
            PertainCubeList.Add(cubeIp);
        }
    }
    class CubeRepository
    {
        public List<CubeType> CubeTypeList { get; private set; }
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


