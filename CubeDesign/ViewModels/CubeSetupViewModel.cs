using System.Net;
using UeiBridge.CubeSetupTypes;

namespace CubeDesign.ViewModels
{
    public class CubeSetupViewModel
    {
        public CubeSetup CubeSetup { get; }
        public IPAddress CubeAddress { get; }
        public string CubeNickname { get; }
        public string CubeTypeId { get; }
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
            this.IsCubeConnected = true;// isCubeConnected;
            //this.CubeAddress = StaticMethods.CubeUrlToIpAddress(CubeSetup.CubeUrl);
            this.CubeNickname = cubesetup.CubeTypeNickname;
            this.CubeTypeId = $"Id={cubesetup.CubeTypeId}";
        }
    }
}