using System.Net;
using UeiBridge.Library.CubeSetupTypes;
using UeiBridge.Library;

namespace CubeDesign.ViewModels
{
    /// <summary>
    /// This model represents a uei-device which is laid in a specific cube-slot.
    /// </summary>
    public class DeviceSetupViewModel : ViewModelBase
    {
        public int SlotNumber => ThisDeviceSetup.SlotNumber;
        public IPAddress EnclosingCubeAddress { get; private set; }
        public int GetCubeId() 
        {
            return ThisDeviceSetup.GetCubeId();
        }
        public string DeviceName => ThisDeviceSetup.DeviceName;
        public DeviceSetup ThisDeviceSetup { get; private set; }
        public bool IsEnabled
        {
            get => ThisDeviceSetup.IsEnabled;
            set
            {
                ThisDeviceSetup.IsEnabled = value;
                RaisePropertyChanged();
            }
        }
        public DeviceSetupViewModel(IPAddress cubeIp, DeviceSetup devSetup)
        {
            this.ThisDeviceSetup = devSetup;
            this.EnclosingCubeAddress = cubeIp;
            this._deviceDesc = DeviceMap2.GetDeviceDesc( ThisDeviceSetup.DeviceName);
        }
        string _deviceDesc;
        public string DeviceDesc => _deviceDesc;

    }
}