using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace UeiBridge.CubeNet
{
    /// <summary>
    /// R&R: Manage CubeRepository class
    /// 1. create new instance of CubeRepository
    /// 2. load/save from/to backing file
    /// 3. some repository help methods
    /// </summary>
    class CubeRepositoryProxy
    {
        //CubeRepository _cubeRepositroy;
        public bool IsRepositoryExist { get; private set; } = false;
        public FileInfo RepositoryBackingFile { get; private set; } = null;
        //internal CubeRepository CubeRepositroy { get => _cubeRepositroy; private set => _cubeRepositroy = value; }
        internal CubeRepository CubeRepositroy { get; set; }

        /// <summary>
        /// Load from backing file.
        /// this method might throw
        /// </summary>
        /// <param name="repFile"></param>
        /// <returns></returns>
        internal bool LoadRepository(FileInfo repFile)
        {
            bool rv = false;
            if (!IsRepositoryExist)
            {
                using (StreamReader reader = repFile.OpenText())
                {
                    CubeRepositroy = JsonConvert.DeserializeObject<CubeRepository>(reader.ReadToEnd());
                    IsRepositoryExist = true;
                    RepositoryBackingFile = repFile;
                    rv = true;
                }
            }
            return rv;
        }
        internal void CreateEmptyRepository()
        {
            if (!IsRepositoryExist)
            {
                CubeRepositroy = new CubeRepository();
                IsRepositoryExist = true;
                RepositoryBackingFile = null;
            }
        }

        /// <summary>
        /// Save to backing file.
        /// this method might throw
        /// </summary>
        /// <param name="repFile"></param>
        /// <returns></returns>
        internal bool SaveRepository(FileInfo repFile)
        {
            bool rc = false;
            string s = JsonConvert.SerializeObject(CubeRepositroy, Formatting.Indented);
            using (StreamWriter fs = repFile.CreateText())
            {
                fs.Write(s);
                RepositoryBackingFile = repFile;
            }
            rc = true;
            return rc;
        }
        internal List<IPAddress> GetAllPertainedCubes()
        {
            List<IPAddress> linearCubeList = new List<IPAddress>();
            foreach (CubeType ct in CubeRepositroy.CubeTypeList)
            {
                foreach (string ip in ct.PertainCubeList)
                {
                    linearCubeList.Add( IPAddress.Parse(ip));
                }
            }
            return linearCubeList;
        }
        
        void f()
        {
            IPAddress ip;
            //ip.GetAddressBytes();
            //IPAddress ip1 = new IPAddress()
        }

        void AddCubeTypeEntry(CubeType ct)
        {
            CubeRepositroy.CubeTypeList.Add(ct);
        }

        internal List<CubeType> GetCubeTypesBySignature(string cubeSignature)
        {
            var l = CubeRepositroy.CubeTypeList.Where(i => i.CubeSignature == cubeSignature);
            return l.ToList();
        }

        internal CubeType AddCubeType( string nickName, string desc, string cubeSignature)
        {
            CubeType ct = new CubeType(nickName, desc);
            ct.SetSignature(cubeSignature);

            CubeRepositroy.CubeTypeList.Add(ct);
            return ct;
        }
    }
}
