using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace UeiBridge.CubeNet
{
    /// <summary>
    /// R&R: Manage CubeRepository object
    /// 1. create new instance of CubeRepository
    /// 2. load/save from/to backing store (json file)
    /// 3. some repository help methods
    /// </summary>
    public class CubeRepositoryProxy
    {
        public bool IsRepositoryLoaded => (CubeRepositroyMain != null);// //get ; private set; } = false;
        public FileInfo RepositoryFile { get; private set; } = null;
        //internal CubeRepository CubeRepositroy { get => _cubeRepositroy; private set => _cubeRepositroy = value; }
        CubeRepository CubeRepositroyMain { get; set; }
        CubeRepository CubeRepositroyClean { get; set; }

        /// <summary>
        /// Load from file.
        /// this method might throw serialization exception.
        /// if rep already open, throw exception
        /// </summary>
        /// <param name="repFile"></param>
        /// <returns></returns>
        public bool LoadRepository(FileInfo repFile)
        {
            bool rv = false;
            if (!IsRepositoryLoaded)
            {
                using (StreamReader reader = repFile.OpenText())
                {
                    CubeRepositroyMain = JsonConvert.DeserializeObject<CubeRepository>(reader.ReadToEnd());
                    RepositoryFile = repFile;
                    rv = true;
                }
                using (StreamReader reader = repFile.OpenText())
                {
                    CubeRepositroyClean = JsonConvert.DeserializeObject<CubeRepository>(reader.ReadToEnd());
                }
            }
            else
            {
                throw new ArgumentException("Repository already loaded");
            }
            return rv;
        }
        
        public void CloseRepository()
        {
            CubeRepositroyMain = null;
            CubeRepositroyClean = null;
        }
        public string GetRepoStatString()
        {
            if (IsRepositoryLoaded)
            {
                int entries = CubeRepositroyMain.CubeTypeList.Count;
                int cubes = GetAllPertainedCubes().Count;
                string result = $"{entries} cube types, {cubes} cubes";
                return result;
            }
            return null;
        }
        /// <summary>
        /// Create and save new rep
        /// </summary>
        /// <param name="repFile"></param>
        public void CreateEmptyRepository(FileInfo repFile)
        {
            if (IsRepositoryLoaded)
            {
                throw new ArgumentException("Repository already loaded");
            }
            CubeRepositroyMain = new CubeRepository();
            CubeRepositroyClean = new CubeRepository();
            RepositoryFile = repFile;

            this.SaveRepository();
        }

        /// <summary>
        /// Save rep to file.
        /// this method might throw
        /// </summary>
        /// <param name="repFile"></param>
        /// <returns></returns>
        public bool SaveRepository()
        {
            bool rc = false;
            string s = JsonConvert.SerializeObject(CubeRepositroyMain, Formatting.Indented);
            using (StreamWriter fs = RepositoryFile.CreateText())
            {
                fs.Write(s);
                rc = true;
            }

            using (StreamReader reader = RepositoryFile.OpenText())
            {
                CubeRepositroyClean = JsonConvert.DeserializeObject<CubeRepository>(reader.ReadToEnd());
            }
            return rc;
        }
        /// <summary>
        /// get linear list of cubes from CubeRep.json
        /// </summary>
        /// <returns></returns>
        internal List<IPAddress> GetAllPertainedCubes()
        {
            if (null==CubeRepositroyMain)
            {
                return null;
            }
            List<IPAddress> linearCubeList = new List<IPAddress>();
            foreach (CubeType ct in CubeRepositroyMain.CubeTypeList)
            {
                foreach (string ip in ct.PertainCubeList)
                {
                    linearCubeList.Add( IPAddress.Parse(ip));
                }
            }
            return linearCubeList;
        }
        
        void AddCubeTypeEntry(CubeType ct)
        {
            CubeRepositroyMain.CubeTypeList.Add(ct);
        }

        internal List<CubeType> GetCubeTypesBySignature(string cubeSignature)
        {
            var l = CubeRepositroyMain.CubeTypeList.Where(i => i.CubeSignature == cubeSignature);
            return l.ToList();
        }
        internal List<CubeType> GetCubeTypeByNickName(string nickname)
        {
            var l = CubeRepositroyMain.CubeTypeList.Where(i => i.NickName == nickname);
            return l.ToList();
        }

        internal CubeType AddCubeType( string nickName, string desc, string cubeSignature)
        {
            CubeType ct = new CubeType(nickName, desc, cubeSignature);

            CubeRepositroyMain.CubeTypeList.Add(ct);
            return ct;
        }

        public bool CheckRepositoryChange()
        {
            if (null==CubeRepositroyMain)
            {
                return false;
            }

            if (CubeRepositroyMain.Equals(CubeRepositroyClean))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        internal List<CubeType> GetCubeTypes()
        {
            return CubeRepositroyMain?.CubeTypeList;
        }
    }
}
