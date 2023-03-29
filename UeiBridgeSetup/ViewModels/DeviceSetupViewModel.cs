using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace UeiBridgeSetup.ViewModels
{
    public class DeviceSetupViewModel : ViewModelBase
    {
        private UeiCube _selectedCube;
        private ObservableCollection<UeiSlot> _slotList = new ObservableCollection<UeiSlot>();
        private UeiSlot _selectedSlot;

        public List<UeiCube> CubeList { get; set; } = new List<UeiCube>();
        public ObservableCollection<UeiSlot> SlotList
        {
            get => _slotList;
            set
            {
                _slotList = value;
                RaisePropertyChanged();
            }
        }
        public UeiSlot SelectedSlot
        {
            get => _selectedSlot;
            set
            {
                _selectedSlot = value;
                RaisePropertyChanged();
            }
        }
        public DeviceSetupViewModel()
        {
            CubeList.Add(new UeiCube(IPAddress.Parse("192.168.100.22"), true));
            CubeList.Add(new UeiCube(IPAddress.Parse("192.168.100.32"), false));
            SelectedCube = CubeList[0];
        }

        public UeiCube SelectedCube
        {
            get => _selectedCube;
            set
            {
                _selectedCube = value;
                RaisePropertyChanged();
                LoadDeviceList(_selectedCube);
            }
        }

        private void LoadDeviceList(UeiCube selectedCube)
        {
            _slotList.Clear();
            if (selectedCube.CubeAddress.Equals(IPAddress.Parse("192.168.100.22")))
            {
                _slotList.Add(new UeiSlot( selectedCube.CubeAddress, 0, new UeiDevice("AO-308", "analog out")));
            }
            if (selectedCube.CubeAddress.Equals(IPAddress.Parse("192.168.100.32")))
            {
                _slotList.Add(new UeiSlot(selectedCube.CubeAddress, 0, new UeiDevice("DIO-403", "digital in/out")));
            }
        }
    }
}
