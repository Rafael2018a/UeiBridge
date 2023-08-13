using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UeiBridge.Library;
using UeiDaq;

namespace UeiBridgeSetup.ViewModels
{
    public class SL508ViewModel : ViewModelBase
    {
        private SerialPortMode _serialMode;
        private CubeSetup _cubeSetup;
        private PhysicalDevice _selectedPhDevice;
        private int _selectedPortIndex = 0;
        private SerialPortSpeed _baudrate;

        public List<string> ChannelList { get; set; } = new List<string>();
        //public string SelectedItem
        //{
        //    get => _selectedItem;
        //    set
        //    {
        //        _selectedItem = value;
        //        SerialChannel sc = _thisDeviceSetup?.Channels.Where(c => c.portname == _selectedItem).FirstOrDefault();
        //        SerialMode = sc.mode;
        //        RaisePropertyChanged();
        //    }
        //}
        public int SelectedPortIndex
        {
            get => _selectedPortIndex;
            set
            {
                _selectedPortIndex = value;
                SL508892Setup setup = _cubeSetup.GetDeviceSetupEntry(_selectedPhDevice.SlotNumber) as SL508892Setup;
                SerialMode = setup.Channels[_selectedPortIndex].mode;
                Baudrate = setup.Channels[_selectedPortIndex].Baudrate;
            }
        }
        public UeiDaq.SerialPortSpeed Baudrate
        {
            get => _baudrate;
            set
            {
                _baudrate = value;
                SL508892Setup setup = _cubeSetup.GetDeviceSetupEntry(_selectedPhDevice.SlotNumber) as SL508892Setup;
                setup.Channels[_selectedPortIndex].Baudrate = _baudrate;
                RaisePropertyChanged();
            }
        }

        public UeiDaq.SerialPortMode SerialMode
        {
            get => _serialMode;
            set
            {
                _serialMode = value;
                SL508892Setup setup = _cubeSetup.GetDeviceSetupEntry(_selectedPhDevice.SlotNumber) as SL508892Setup;
                setup.Channels[_selectedPortIndex].mode = _serialMode;
                RaisePropertyChanged();
            }
        }

        public SL508ViewModel(List<CubeSetup> cubeSetupList, PhysicalDevice selectedPhDevice)
        {
            this._selectedPhDevice = selectedPhDevice;
            // find setup entry for 'selectedPhDevice'
            var csl = cubeSetupList.Where(cs => cs.GetCubeId() == selectedPhDevice.GetCubeId());
            this._cubeSetup = csl.FirstOrDefault();
            
            System.Diagnostics.Debug.Assert(this._cubeSetup != null);

            SL508892Setup thisDeviceSetup = selectedPhDevice.ThisDeviceSetup as SL508892Setup;
            System.Diagnostics.Debug.Assert(null != thisDeviceSetup);

            foreach (var channel in thisDeviceSetup.Channels)
            {
                ChannelList.Add($"Com{channel.ChannelIndex}");
            }

            SelectedPortIndex = 0;

        }
    }
}
