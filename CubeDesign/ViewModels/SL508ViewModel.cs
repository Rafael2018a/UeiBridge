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
        //private CubeSetup _thisCubeSetup;
        private SL508892Setup _thisDeviceSetup;

        public List<string> ChannelList { get; set; } = new List<string>();
        private int _selectedPortIndex = 0;
        public int SelectedPortIndex
        {
            get => _selectedPortIndex;
            set
            {
                _selectedPortIndex = value;
                SerialMode = _thisDeviceSetup.Channels[_selectedPortIndex].Mode;
                Baudrate = _thisDeviceSetup.Channels[_selectedPortIndex].Baudrate;
            }
        }
        private SerialPortSpeed _baudrate;
        public UeiDaq.SerialPortSpeed Baudrate
        {
            get => _baudrate;
            set
            {
                _baudrate = value;
                _thisDeviceSetup.Channels[_selectedPortIndex].Baudrate = _baudrate;
                RaisePropertyChanged();
            }
        }
        private SerialPortMode _serialMode;
        public UeiDaq.SerialPortMode SerialMode
        {
            get => _serialMode;
            set
            {
                _serialMode = value;
                _thisDeviceSetup.Channels[_selectedPortIndex].Mode = _serialMode;
                RaisePropertyChanged();
            }
        }

        public SL508ViewModel( DeviceSetupViewModel selectedPhDevice)
        {
            // find setup entry for selected device'
            //var csl = cubeSetupList.Where(cs => cs.GetCubeId() == selectedPhDevice.GetCubeId());
            //_thisCubeSetup = csl.FirstOrDefault();
            //System.Diagnostics.Debug.Assert( _thisCubeSetup != null);

            _thisDeviceSetup = selectedPhDevice.ThisDeviceSetup as SL508892Setup;
            System.Diagnostics.Debug.Assert(null != _thisDeviceSetup);

            foreach (var channel in _thisDeviceSetup.Channels)
            {
                ChannelList.Add($"Com{channel.ChannelIndex}");
            }

            SelectedPortIndex = 0;

        }
    }
}
