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
        private Config2 _mainConfig;
        private SlotDeviceModel _selectedSlot;
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
                SL508892Setup setup = _mainConfig.GetSetupEntryForDevice(_selectedSlot.ThisDeviceSetup.CubeUrl, _selectedSlot.SlotNumber) as SL508892Setup;
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
                SL508892Setup setup = _mainConfig.GetSetupEntryForDevice(_selectedSlot.ThisDeviceSetup.CubeUrl, _selectedSlot.SlotNumber) as SL508892Setup;
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
                SL508892Setup setup = _mainConfig.GetSetupEntryForDevice(_selectedSlot.ThisDeviceSetup.CubeUrl, _selectedSlot.SlotNumber) as SL508892Setup;
                setup.Channels[_selectedPortIndex].mode = _serialMode;
                RaisePropertyChanged();
            }
        }

        public SL508ViewModel(Config2 mainConfig, SlotDeviceModel selectedSlot)
        {
            this._mainConfig = mainConfig;
            this._selectedSlot = selectedSlot;

            SL508892Setup _thisDeviceSetup;

            _thisDeviceSetup = selectedSlot.ThisDeviceSetup as SL508892Setup;
            if (null == _thisDeviceSetup)
            {
                throw new ArgumentNullException();
            }
            //mainConfig.GetSetupEntryForDevice(selectedSlot.ThisDeviceSetup.CubeId, selectedSlot.DeviceName);
            if (_thisDeviceSetup.Channels.Count > 0)
            {
                foreach (var channel in _thisDeviceSetup.Channels)
                {
                    ChannelList.Add(channel.portname);
                }

                SelectedPortIndex = 0;
            }
            //SerialMode = UeiDaq.SerialPortMode.RS485HalfDuplex;

        }
    }
}
