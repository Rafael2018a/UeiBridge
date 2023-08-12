using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UeiBridge.Library;

namespace UeiBridgeSetup.ViewModels
{
    public class SystemSetupViewModel : ViewModelBase
    {
        #region === privates ===
        private CubeSetupViewModel _selectedCube;
        private ObservableCollection<PhysicalDevice> _phDeviceList = new ObservableCollection<PhysicalDevice>();
        private PhysicalDevice _selectedPhDevice;
        private ViewModelBase _selectedViewModel;
        private EndPointViewModel _destinationEndPointViewModel;
        private EndPointViewModel _localEndPointViewModel;
        private List<CubeSetup> _cubeSetupList;
        #endregion
        #region === publics ===
        public EndPointViewModel LocalEndPointViewModel
        {
            get => _localEndPointViewModel;
            set
            {
                _localEndPointViewModel = value;
                RaisePropertyChanged();
            }
        }
        public EndPointViewModel DestinationEndPointViewModel
        {
            get => _destinationEndPointViewModel;
            set
            {
                _destinationEndPointViewModel = value;
                RaisePropertyChanged();
            }
        }// = new EndPointViewModel(EndPointLocation.Local);
        public List<CubeSetupViewModel> CubeSetupVMList { get; set; } = new List<CubeSetupViewModel>();
        public ObservableCollection<PhysicalDevice> PhDeviceList
        {
            get => _phDeviceList;
            set
            {
                _phDeviceList = value;
                RaisePropertyChanged();
            }
        }
        public PhysicalDevice SelectedPhDevice
        {
            get => _selectedPhDevice;
            set
            {
                _selectedPhDevice = value;
                RaisePropertyChanged();
                if (_selectedPhDevice != null)
                {
                    LocalEndPointViewModel = new EndPointViewModel(EndPointLocation.Local, _selectedPhDevice.ThisDeviceSetup.LocalEndPoint);
                    DestinationEndPointViewModel = new EndPointViewModel(EndPointLocation.Dest, _selectedPhDevice.ThisDeviceSetup.DestEndPoint);
                    SelectedViewModel = GetDeviceViewModel(_selectedPhDevice);
                }
            }
        }

        //private ViewModelBase GetViewModelBySlot(SlotDeviceModel selectedSlot)
        //{
        //    string devicename = _selectedSlot.DeviceName.ToLower();
        //    if (devicename.StartsWith("sl-508"))
        //    {
        //        return new SL508ViewModel();
        //    }
        //    if (devicename == DeviceMap2.AO308Literal.ToLower())
        //    {
        //        return new AO308ViewModel();
        //    }
        //    if (devicename.StartsWith("dio-403"))
        //    {
        //        return new DIO403ViewModel();
        //    }
        //    if (devicename.StartsWith("ai-201"))
        //    {
        //        return new AI201ViewModel();
        //    }
        //    if (devicename.StartsWith("dio-470"))
        //    {
        //        return new DIO470ViewModel();
        //    }
        //    return null;
        //}
        private ViewModelBase GetDeviceViewModel(PhysicalDevice selectedPhDevice)
        {
            switch (_selectedPhDevice.DeviceName)
            {
                case DeviceMap2.SL508Literal:
                    return new SL508ViewModel(_cubeSetupList, selectedPhDevice);
                case DeviceMap2.AO308Literal:
                    return new AO308ViewModel();
                case DeviceMap2.DIO403Literal:
                    return new DIO403ViewModel();
                case DeviceMap2.AI201Literal:
                    return new AI201ViewModel();
                case DeviceMap2.DIO470Literal:
                    return new DIO470ViewModel();
                default:
                    return null;
            }
        }

        public ViewModelBase SelectedViewModel
        {
            get => _selectedViewModel;
            set
            {
                _selectedViewModel = value;
                RaisePropertyChanged();
            }
        }
        #endregion
        #region === commands ===
        public DelegateCommand AddCubeCommand { get; }
        #endregion

        public SystemSetupViewModel(List<CubeSetup> cubeSetupList)
        {
            _cubeSetupList = cubeSetupList;
           
            foreach (CubeSetup cs in _cubeSetupList)
            {
                CubeSetupVMList.Add(new CubeSetupViewModel( cs, false));
            }
            if (CubeSetupVMList.Count > 0)
            {
                SelectedCube = CubeSetupVMList[0];
            }
            //SelectedViewModel = new SL508ViewModel();

            // load commands
            AddCubeCommand = new DelegateCommand(new Action<object>(AddCubeCmd), new Func<object, bool>(CanAddCubeCmd));
        }

        private bool CanAddCubeCmd(object arg)
        {
            //throw new NotImplementedException();
            return true;
        }

        private void AddCubeCmd(object obj)
        {
            //throw new NotImplementedException();
            Views.AddCubeDialog d1 = new Views.AddCubeDialog();
            if (true == d1.ShowDialog())
            {

            }
        }

        void AddCube(IPAddress cubeIp)
        {
            throw new NotImplementedException();

            //string uri = $"pdna://{cubeIp.ToString()}";
            //var cs = new CubeSetup(new List<UeiDeviceAdapter>(), uri);
            //_mainConfig.UeiCubes.Add(cs);

            //CubeList.Add(new CubeSetupViewModel(cs, false));
        }

        //private void LoadCubeList(CubeSetup cubeConfig)
        //{
        //    CubeSetupVMList.Add(new CubeSetupViewModel(cubeConfig, false));
            //foreach (CubeSetup cubesetup in mainConfig.CubeSetupList)
            //{
            //    //if (!cubesetup.CubeUrl.ToLower().StartsWith("simu"))
            //    {
            //        CubeList.Add(new CubeSetupViewModel(cubesetup, true));
            //    }
            //}
//        }
        public CubeSetupViewModel SelectedCube
        {
            get => _selectedCube;
            set
            {
                _selectedCube = value;
                LoadDeviceList(_selectedCube);
                RaisePropertyChanged();
            }
        }

        private void LoadDeviceList(CubeSetupViewModel selectedCube)
        {
            _phDeviceList.Clear();
            foreach (DeviceSetup devSetup in selectedCube.CubeSetup.DeviceSetupList)
            {
                _phDeviceList.Add(new PhysicalDevice(selectedCube.CubeAddress, devSetup));
            }

            if (selectedCube.CubeSetup.DeviceSetupList.Count > 0)
            {
                SelectedPhDevice = _phDeviceList[0];
            }
        }
    }
}
