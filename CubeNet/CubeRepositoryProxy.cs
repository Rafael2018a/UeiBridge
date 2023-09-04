using System.Collections.Generic;
using System.IO;
using System.Net;

namespace UeiBridge.CubeNet
{
    class CubeRepositoryProxy
    {
        CubeRepository _cubeRepositroy;
        internal CubeRepository LoadRepository(FileInfo repFile)
        {
            _cubeRepositroy = new CubeRepository();

            {
                CubeType ct = new CubeType("cubeName", "this is good cube");
                ct.AddCube(IPAddress.Parse("192.168.100.2"));
                AddCubeTypeEntry(ct);
            }
            return _cubeRepositroy;
        }

        internal List<IPAddress> GetAllPertainedCubes()
        {
            List<IPAddress> linearCubeList = new List<IPAddress>();
            foreach( CubeType ct in _cubeRepositroy.CubeTypeList)
            {
                foreach(IPAddress ip in ct.PertainCubeList)
                {
                    linearCubeList.Add(ip);
                }
            }
            return linearCubeList;
        }

        void AddCubeTypeEntry( CubeType ct)
        {
            _cubeRepositroy.CubeTypeList.Add(ct);
        }
    }
}
