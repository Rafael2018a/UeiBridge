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
        CubeRepository _cubeRepositroy;
        public bool IsRepositoryExist { get; private set; } = false;
        public FileInfo RepositoryBackingFile { get; private set; } = null;
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
                    _cubeRepositroy = JsonConvert.DeserializeObject<CubeRepository>(reader.ReadToEnd());
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
                _cubeRepositroy = new CubeRepository();
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
            string s = JsonConvert.SerializeObject(_cubeRepositroy, Formatting.Indented);
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
            foreach (CubeType ct in _cubeRepositroy.CubeTypeList)
            {
                foreach (IPAddress ip in ct.PertainCubeList)
                {
                    linearCubeList.Add(ip);
                }
            }
            return linearCubeList;
        }

        void AddCubeTypeEntry(CubeType ct)
        {
            _cubeRepositroy.CubeTypeList.Add(ct);
        }

        internal List<CubeType> GetCubeTypesBySignature(string cubeSignature)
        {
            var l = _cubeRepositroy.CubeTypeList.Where(i => i.CubeSignature == cubeSignature);
            return l.ToList();
        }

        internal void AddCubeType( string nickName, string desc, string cubeSignature)
        {
            CubeType ct = new CubeType(nickName, desc);
            ct.SetSignature(cubeSignature);

            _cubeRepositroy.CubeTypeList.Add(ct);
        }
    }
}
