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
        private ObservableCollection<UeiSlotViewModel> _slotList = new ObservableCollection<UeiSlotViewModel>();
        private UeiSlotViewModel _selectedSlot;
        private ViewModelBase _selectedViewModel;
        #endregion
        #region === publics ===
        public EndPointViewModel LocalEndPointViewModel { get;} = new EndPointViewModel(EndPointLocation.Local);
        public EndPointViewModel DestinationEndPointViewModel { get; } = new EndPointViewModel(EndPointLocation.Local);
        public List<CubeSetupViewModel> CubeList { get; set; } = new List<CubeSetupViewModel>();
        public ObservableCollection<UeiSlotViewModel> SlotList
        {
            get => _slotList;
            set
            {
                _slotList = value;
                RaisePropertyChanged();
            }
        }
        public UeiSlotViewModel SelectedSlot
        {
            get => _selectedSlot;
            set
            {
                _selectedSlot = value;
                RaisePropertyChanged();
                if (_selectedSlot != null)
                {
                    LocalEndPointViewModel.SetEndPoint(_selectedSlot.ThisDeviceSetup.LocalEndPoint?.ToIpEp());
                    DestinationEndPointViewModel.SetEndPoint(_selectedSlot.ThisDeviceSetup.DestEndPoint?.ToIpEp());
                    SelectedViewModel = getViewModelBySlot(_selectedSlot);
                }
            }
        }

        private ViewModelBase getViewModelBySlot(UeiSlotViewModel selectedSlot)
        {
            string devicename = _selectedSlot.DeviceName.ToLower();
            if (devicename.StartsWith("sl-508"))
            {
                return new SL508ViewModel();
            }
            if (devicename.StartsWith("ao-308"))
            {
                return new AO308ViewModel();
            }
            if (devicename.StartsWith("dio-403"))
            {
                return new DIO403ViewModel();
            }
            if (devicename.StartsWith("ai-201"))
            {
                return new AI201ViewModel();
            }
            if (devicename.StartsWith("dio-470"))
            {
                return new DIO470ViewModel();
            }    
            return null;
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
        public SystemSetupViewModel()
        {
            LoadCubeList();
            if (CubeList.Count > 0)
            {
                SelectedCube = CubeList[0];
            }
            SelectedViewModel = new SL508ViewModel();
        }

        void AddCube(IPAddress cubeIp)
        {
            string uri = $"pdna://{cubeIp.ToString()}";
            var cs = new CubeSetup(uri);
            Config2.Instance.UeiCubes.Add( cs);
            
            CubeList.Add(new CubeSetupViewModel(cs, false));
        }

        private void LoadCubeList()
        {
            if (Config2.IsConfigFileExist())
            {
                foreach (CubeSetup cubesetup in Config2.Instance.UeiCubes)
                {
                    if (!cubesetup.CubeUrl.ToLower().StartsWith("simu"))
                    {
                        CubeList.Add(new CubeSetupViewModel(cubesetup, false));
                    }
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
            _slotList.Clear();
            foreach( var dev in selectedCube.CubeSetup.DeviceSetupList)
            {
                _slotList.Add(new UeiSlotViewModel(selectedCube.CubeAddress, dev));
            }

            if (selectedCube.CubeSetup.DeviceSetupList.Count > 0)
            {
                SelectedSlot = _slotList[0];
            }
        }
    }
}
