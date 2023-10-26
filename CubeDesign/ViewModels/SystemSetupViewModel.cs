using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UeiBridge.CubeSetupTypes;
using UeiBridge.Library;

namespace CubeDesign.ViewModels
{
    public class SystemSetupViewModel : ViewModelBase
    {
        #region === privates ===
        private CubeSetupViewModel _selectedCube;
        private ObservableCollection<DeviceSetupViewModel> _deviceSetupVMList = new ObservableCollection<DeviceSetupViewModel>();
        private DeviceSetupViewModel _deviceSetupViewModel;
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
        public ObservableCollection<CubeSetupViewModel> CubeSetupVMList { get; set; } = new ObservableCollection<CubeSetupViewModel>();
        public ObservableCollection<DeviceSetupViewModel> DeviceSetupVMList
        {
            get => _deviceSetupVMList;
            set
            {
                _deviceSetupVMList = value;
                RaisePropertyChanged();
            }
        }
        public DeviceSetupViewModel SelectedDeviceSetupVM
        {
            get => _deviceSetupViewModel;
            set
            {
                _deviceSetupViewModel = value;
                RaisePropertyChanged();
                if (_deviceSetupViewModel != null)
                {
                    LocalEndPointViewModel = new EndPointViewModel(EndPointLocation.Local, _deviceSetupViewModel.ThisDeviceSetup.LocalEndPoint);
                    DestinationEndPointViewModel = new EndPointViewModel(EndPointLocation.Dest, _deviceSetupViewModel.ThisDeviceSetup.DestEndPoint);
                    SelectedDeviceViewModel = GetDeviceViewModel(_deviceSetupViewModel);
                }
            }
        }

        private ViewModelBase GetDeviceViewModel(DeviceSetupViewModel selectedDevice)
        {
            switch (_deviceSetupViewModel.DeviceName)
            {
                case DeviceMap2.SL508Literal:
                    return new SL508ViewModel( selectedDevice);
                case DeviceMap2.AO308Literal:
                    return new AO308ViewModel();
                case DeviceMap2.DIO403Literal:
                    return new DIO403ViewModel();
                case DeviceMap2.AI201Literal:
                    return new AI201ViewModel();
                case DeviceMap2.DIO470Literal:
                    return new DIO470ViewModel();
                case DeviceMap2.CAN503Literal:
                    return new CAN503ViewModel();
                default:
                    return null;
            }
        }

        public ViewModelBase SelectedDeviceViewModel
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
        public RelayCommand AddCubeCommand { get; }
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
            AddCubeCommand = new RelayCommand(new Action<object>(AddCubeCmd), new Predicate<object>(CanAddCubeCmd));
        }

        private bool CanAddCubeCmd(object arg)
        {
            //throw new NotImplementedException();
            return true;
        }

        private void AddCubeCmd(object obj)
        {
            //throw new NotImplementedException();
            //Views.AddCubeDialog d1 = new Views.AddCubeDialog();
            //if (true == d1.ShowDialog())
            {
                CubeSetupLoader csl = new CubeSetupLoader();
                csl.LoadSetupFile( new System.IO.FileInfo("Cube3.config"));
                //CubeSetup cs = CubeSetup.LoadCubeSetupFromFile( new System.IO.FileInfo( "Cube3.config"));
                if (null != csl.CubeSetupMain)
                {
                    _cubeSetupList.Add(csl.CubeSetupMain);
                    CubeSetupVMList.Add(new CubeSetupViewModel(csl.CubeSetupMain, false));
                }
            }
        }
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
            _deviceSetupVMList.Clear();
            foreach (DeviceSetup devSetup in selectedCube.CubeSetup.DeviceSetupList)
            {
                _deviceSetupVMList.Add(new DeviceSetupViewModel(selectedCube.CubeAddress, devSetup));
            }

            if (selectedCube.CubeSetup.DeviceSetupList.Count > 0)
            {
                SelectedDeviceSetupVM = _deviceSetupVMList[0];
            }
        }
    }
}
