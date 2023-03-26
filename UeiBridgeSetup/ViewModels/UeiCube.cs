using System.Net;

namespace UeiBridgeSetup.ViewModels
{
    public class UeiCube
    {
        public IPAddress CubeAddress { get; }
        public bool IsCubeConnected { get; }
        //public string CubeName => CubeAddress.ToString();

        public UeiCube(IPAddress cubeAddress, bool isCubeConnected)
        {
            CubeAddress = cubeAddress;
            IsCubeConnected = isCubeConnected;
        }
    }
}