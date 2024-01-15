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
        public int MessageTimeoutUS
        {
            get => _thisDeviceSetup.Channels[_selectedPortIndex].ChannelActivityTimeoutUs;
            set
            {
                _thisDeviceSetup.Channels[_selectedPortIndex].ChannelActivityTimeoutUs = value;
                RaisePropertyChanged();
            }
        }
        public bool FilterBySyncBytes
        {
            get => _thisDeviceSetup.Channels[_selectedPortIndex].FilterBySyncBytes;
            set
            {
                _thisDeviceSetup.Channels[_selectedPortIndex].FilterBySyncBytes = value;
                RaisePropertyChanged();
            }
        }
        public string HexSyncByte0
        {
            get { return _thisDeviceSetup.Channels[_selectedPortIndex].SyncByte0.ToString("X2"); }
            set
            {
                Byte result;
                if (Byte.TryParse(value, System.Globalization.NumberStyles.HexNumber, null, out result))
                {
                    _thisDeviceSetup.Channels[_selectedPortIndex].SyncByte0 = result;
                }
            }
        }
        public string HexSyncByte1
        {
            get { return _thisDeviceSetup.Channels[_selectedPortIndex].SyncByte1.ToString("X2"); }
            set
            {
                Byte result;
                if (Byte.TryParse(value, System.Globalization.NumberStyles.HexNumber, null, out result))
                {
                    _thisDeviceSetup.Channels[_selectedPortIndex].SyncByte1 = result;
                }
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
