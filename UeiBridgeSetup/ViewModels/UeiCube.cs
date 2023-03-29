using System.Net;

namespace UeiBridgeSetup.ViewModels
{
    public class UeiCube
    {
        public IPAddress CubeAddress { get; }
        public bool IsCubeConnected { get; }
        public bool IsCubeNotConnected 
        { 
            get 
            { 
                return !IsCubeConnected; 
            } 
        }
        public bool IsSimulationCube { get; } = false;
        public UeiCube(IPAddress cubeAddress, bool isCubeConnected)
        {
            CubeAddress = cubeAddress;
            IsCubeConnected = isCubeConnected;
        }
        public UeiCube()
        {
            IsSimulationCube = true;
        }
    }
}