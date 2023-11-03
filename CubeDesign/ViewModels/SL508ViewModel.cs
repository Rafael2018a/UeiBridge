// Ignore Spelling: Baudrate

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UeiBridge.CubeSetupTypes;
using UeiBridge.Library;
using UeiDaq;

namespace CubeDesign.ViewModels
{
    public class SL508ViewModel : ViewModelBase
    {
        private SL508892Setup _thisDeviceSetup; // the "model"

        public List<SerialChannelSetup> ChannelList 
        { 
            get => _thisDeviceSetup.Channels;
        } 

        private int _selectedPortIndex = 0;
        public int SelectedPortIndex
        {
            get => _selectedPortIndex;
            set
            {
                _selectedPortIndex = value;
            }
        }
        public UeiDaq.SerialPortSpeed Baudrate
        {
            get => _thisDeviceSetup.Channels[_selectedPortIndex].Baudrate;
            set
            {
                _thisDeviceSetup.Channels[_selectedPortIndex].Baudrate = value;
                RaisePropertyChanged();
            }
        }
        public UeiDaq.SerialPortMode SerialMode
        {
            get => _thisDeviceSetup.Channels[_selectedPortIndex].Mode;
            set
            {
                _thisDeviceSetup.Channels[_selectedPortIndex].Mode = value;
                RaisePropertyChanged();
            }
        }
        public bool EnableChannel
        {
            get => _thisDeviceSetup.Channels[_selectedPortIndex].IsEnabled;
            set
            {
                _thisDeviceSetup.Channels[_selectedPortIndex].IsEnabled = value;
                RaisePropertyChanged();
            }
        }
        public UeiDaq.SerialPortParity Parity
        {
            get => _thisDeviceSetup.Channels[_selectedPortIndex].Parity;
            set
            {
                _thisDeviceSetup.Channels[_selectedPortIndex].Parity = value;
                RaisePropertyChanged();
            }
        }
        public UeiDaq.SerialPortStopBits Stopbits
        {
            get => _thisDeviceSetup.Channels[_selectedPortIndex].Stopbits;
            set
            {
                _thisDeviceSetup.Channels[_selectedPortIndex].Stopbits = value;
                RaisePropertyChanged();
            }
        }
        public SL508ViewModel( DeviceSetupViewModel selectedPhDevice)
        {
            _thisDeviceSetup = selectedPhDevice.ThisDeviceSetup as SL508892Setup;
            System.Diagnostics.Debug.Assert(null != _thisDeviceSetup);
            SelectedPortIndex = 0;
        }
    }
}
