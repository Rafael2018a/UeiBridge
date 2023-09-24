using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace UeiBridge.CubeNet
{
    class CubeType: IEquatable<CubeType>
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

        public bool Equals(CubeType other)
        {
            bool a1 = this.CubeSignature.Equals(other.CubeSignature);
            bool a2 = this.NickName.Equals(other.NickName);
            bool a3 = this.TypeId == other.TypeId;
            bool a4 = this.Desc.Equals(other.Desc);
            bool a5 = this.PertainCubeList.SequenceEqual(other.PertainCubeList);

            return (a1 && a2 && a3 && a4 && a5);
        }
    }
    class CubeRepository: IEquatable<CubeRepository>
    {
        public List<CubeType> CubeTypeList { get; set; }
        public CubeRepository()
        {
            CubeTypeList = new List<CubeType>();
        }

        public bool Equals(CubeRepository other)
        {
            return this.CubeTypeList[0].Equals(other.CubeTypeList[0]);
            //return this.CubeTypeList.SequenceEqual(other.CubeTypeList);
        }
    }

//    SlotMap: “DIO403- SL508- SL508-  SL508- SL508- DIO452”
//NickName: “mcc”
//TypeId: 1
//“Desc”: “this cube should connect to mcc”
//CubeList: [“192.168.100,3”, “192.168.100,15”, “192.168.100,17”]
}


