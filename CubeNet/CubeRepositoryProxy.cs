// Ignore Spelling: json

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
    /// 2. load/save from/to json file
    /// 3. some repository help methods
    /// </summary>
    public class CubeRepositoryProxy
    {
        //public bool IsRepositoryLoaded { get; set; }// => (CubeRepositroyMain != null);// //get ; private set; } = false;
        //public FileInfo RepositoryFile { get; private set; } = null;
        //internal CubeRepository CubeRepositroy { get => _cubeRepositroy; private set => _cubeRepositroy = value; }
        public CubeRepository CubeRepositoryMain { get; set; }
        private CubeRepository CubeRepositoryClean { get; set; }

        public CubeRepositoryProxy()
        {
            CubeRepositoryMain = new CubeRepository();
            CubeRepositoryClean = new CubeRepository();
        }
        public CubeRepositoryProxy(Stream jsonStream)
        {
            jsonStream.Seek(0, SeekOrigin.Begin);
            using (StreamReader reader = new StreamReader(jsonStream))
            {
                CubeRepositoryMain = JsonConvert.DeserializeObject<CubeRepository>(reader.ReadToEnd());

                jsonStream.Seek(0, SeekOrigin.Begin);
                CubeRepositoryClean = JsonConvert.DeserializeObject<CubeRepository>(reader.ReadToEnd());
            }

        }
#if dont
        /// <summary>
        /// Load from file.
        /// this method might throw serialization exception.
        /// if rep already open, throw exception
        /// </summary>
        /// <param name="repFile"></param>
        /// <returns></returns>
        public bool LoadRepository1(FileInfo repFile)
        {
            bool rv = false;
            if (!IsRepositoryLoaded)
            {
                using (StreamReader reader = repFile.OpenText())
                {
                    CubeRepositoryMain = JsonConvert.DeserializeObject<CubeRepository>(reader.ReadToEnd());
                    //RepositoryFile = repFile;
                    rv = true;
                }
                using (StreamReader reader = repFile.OpenText())
                {
                    CubeRepositoryClean = JsonConvert.DeserializeObject<CubeRepository>(reader.ReadToEnd());
                }
            }
            else
            {
                throw new ArgumentException("Repository already loaded");
            }
            return rv;
        }
        public bool LoadRepository(Stream jsonStream)
        {
            bool rv = false;
            if (!IsRepositoryLoaded)
            {
                using (StreamReader reader = new StreamReader(jsonStream))
                {
                    CubeRepositoryMain = JsonConvert.DeserializeObject<CubeRepository>(reader.ReadToEnd());
                    //RepositoryFile = repFile;
                    rv = true;
                }
                jsonStream.Seek(0, SeekOrigin.Begin);
                using (StreamReader reader = new StreamReader(jsonStream))
                {
                    CubeRepositoryClean = JsonConvert.DeserializeObject<CubeRepository>(reader.ReadToEnd());
                }
            }
            else
            {
                throw new ArgumentException("Repository already loaded");
            }
            return rv;
        }
#endif
        public void CloseRepository()
        {
            CubeRepositoryMain = null;
            CubeRepositoryClean = null;
        }
        public string GetRepoStatString()
        {
            //if (IsRepositoryLoaded)
            //{
            int entries = CubeRepositoryMain.CubeTypeList.Count;
            int cubes = GetAllPertainedCubes().Count;
            string result = $"{entries} cube types, {cubes} cubes";
            return result;
            //}
            //return null;
        }
#if dont
        /// <summary>
        /// Create and save new rep
        /// </summary>
        public void CreateEmptyRepository1()
        {
            if (IsRepositoryLoaded)
            {
                throw new ArgumentException("Repository already loaded");
            }
            CubeRepositoryMain = new CubeRepository();
            CubeRepositoryClean = new CubeRepository();
            //RepositoryFile = repFile;

            //this.SaveRepository();
        }
#endif
        /// <summary>
        /// Save rep to file.
        /// this method might throw
        /// </summary>
        /// <param name="repFile"></param>
        /// <returns></returns>
        public bool SaveRepository(FileInfo targetFile)
        {
            bool rc = false;
            string s = JsonConvert.SerializeObject(CubeRepositoryMain, Formatting.Indented);
            using (StreamWriter fs = targetFile.CreateText())
            {
                fs.Write(s);
                rc = true;
            }

            using (StreamReader reader = targetFile.OpenText())
            {
                CubeRepositoryClean = JsonConvert.DeserializeObject<CubeRepository>(reader.ReadToEnd());
            }
            return rc;
        }
        /// <summary>
        /// get linear list of cubes from CubeRep.json
        /// </summary>
        /// <returns></returns>
        internal List<IPAddress> GetAllPertainedCubes()
        {
            if (null == CubeRepositoryMain)
            {
                return null;
            }
            List<IPAddress> linearCubeList = new List<IPAddress>();
            foreach (CubeType ct in CubeRepositoryMain.CubeTypeList)
            {
                foreach (string ip in ct.PertainCubeList)
                {
                    linearCubeList.Add(IPAddress.Parse(ip));
                }
            }
            return linearCubeList;
        }

        void AddCubeTypeEntry(CubeType ct)
        {
            CubeRepositoryMain.CubeTypeList.Add(ct);
        }

        internal List<CubeType> GetCubeTypesBySignature(string cubeSignature)
        {
            var l = CubeRepositoryMain.CubeTypeList.Where(i => i.CubeSignature == cubeSignature);
            return l.ToList();
        }
        internal List<CubeType> GetCubeTypeByNickName(string nickname)
        {
            var l = CubeRepositoryMain.CubeTypeList.Where(i => i.NickName == nickname);
            return l.ToList();
        }

        internal CubeType AddCubeType(string nickName, string desc, string cubeSignature)
        {
            CubeType ct = new CubeType(nickName, desc, cubeSignature);

            CubeRepositoryMain.CubeTypeList.Add(ct);
            return ct;
        }

        public bool CheckRepositoryChange()
        {
            if (null == CubeRepositoryMain)
            {
                return false;
            }

            if (CubeRepositoryMain.Equals(CubeRepositoryClean))
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
            return CubeRepositoryMain?.CubeTypeList;
        }
    }
}
