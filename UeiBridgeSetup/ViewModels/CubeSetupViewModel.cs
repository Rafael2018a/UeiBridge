using System.Net;
using UeiBridge.Library;

namespace UeiBridgeSetup.ViewModels
{
    public class CubeSetupViewModel
    {
        public CubeSetup CubeSetup { get; }
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
        public CubeSetupViewModel(CubeSetup cubesetup, bool isCubeConnected)
        {
            this.CubeSetup = cubesetup;
            this.IsCubeConnected = isCubeConnected;
            this.CubeAddress = StaticMethods.CubeUrlToIpAddress(CubeSetup.CubeUrl);
        }
    }
}