using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace UeiBridge.CubeNet
{
    /// <summary>
    /// R&R: Manage CubeRepository class
    /// 1. create new instance
    /// 2. load/save from/to file
    /// 3. some repository help methods
    /// </summary>
    class CubeRepositoryProxy
    {
        CubeRepository _cubeRepositroy;
        public bool IsRepositoryExist { get; private set; } = false;
        internal bool LoadRepository(FileInfo repFile)
        {
            bool rv = false;
            if (!IsRepositoryExist)
            {
                try
                {
                    using (StreamReader reader = repFile.OpenText())
                    {
                        _cubeRepositroy = JsonConvert.DeserializeObject<CubeRepository>(reader.ReadToEnd());
                        IsRepositoryExist = true;
                        rv = true;
                    }
                }
                catch(Exception ex)
                {
                    // nothing to do here
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
            }
        }

        internal bool SaveRepository(FileInfo repFile)
        {
            bool rc = false;
            try
            {
                string s = JsonConvert.SerializeObject(_cubeRepositroy, Formatting.Indented);
                using (StreamWriter fs = repFile.CreateText())
                {
                    fs.Write(s);
                }
                rc = true;
            }
            catch(Exception ex)
            {

            }
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

    }
}
