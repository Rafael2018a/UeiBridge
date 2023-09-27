﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace UeiBridge.CubeNet
{
    public class CubeType: IEquatable<CubeType>
    {
        public string CubeSignature { get; set; }
        public string NickName { get; set; }
        public int TypeId { get; set; }
        public string Desc { get; set; }
        public List<string> PertainCubeList { get; set; }

        public CubeType(string nickName, string desc, string signature)
        {
            NickName = nickName;
            Desc = desc;
            CubeSignature = signature;
            PertainCubeList = new List<string>();
        }
        public void AddCube( System.Net.IPAddress cubeIp)
        {
            PertainCubeList.Add(cubeIp.ToString());
        }

        public bool Equals(CubeType other)
        {
            bool a1 = this.CubeSignature == other.CubeSignature;
            bool a2 = this.NickName == other.NickName;
            bool a3 = this.TypeId == other.TypeId;
            bool a4 = this.Desc == other.Desc;
            bool a5 = this.PertainCubeList.SequenceEqual(other.PertainCubeList);

            return (a1 && a2 && a3 && a4 && a5);
        }
    }
}


