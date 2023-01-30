using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using ByteStreamer3.Util;

namespace ByteStreamer3
{
    class MainViewModel : System.ComponentModel.INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public MainViewModel()
        {
            LoadCommands();
        }

        void RaisePropertyChangedEvent(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        string _destinationAddress = "10.10.10.10";
        public string DestinationAddress
        {
            get => _destinationAddress;
            set
            {
                _destinationAddress = value;
                RaisePropertyChangedEvent("DestinationAddress");
            }
        }

        private void LoadCommands()
        {
            StartPlayCommand = new Utilities.RelayCommand(StartPlay, CanStartPlay);
            //StopPlayCommand = new Utilities.RelayCommand(StopPlay, CanStopPlay);
        }
        private int _radBtnId = 1;
        public int IsSuccess
        {
            get
            { return _radBtnId; }
            set
            { _radBtnId = value; }
        }

        bool _isPlaying;
        public bool IsPlaying
        {
            get => _isPlaying;
            set
            {
                _isPlaying = value;
                RaisePropertyChangedEvent("IsPlaying");
            }
        }

        void StartPlay(object obj)
        {

        }
        bool CanStartPlay(object obj)
        {
            return (IsPlaying == false);
        }

        #region ICommands =====
        private Utilities.RelayCommand _startPlayCommand;
        public Utilities.RelayCommand StartPlayCommand
        {
            get => _startPlayCommand;
            set => _startPlayCommand = value;
        }
        private Utilities.RelayCommand _stopPlayCommand;
        public Utilities.RelayCommand StopPlayCommand
        {
            get { return _stopPlayCommand; }
            set { _stopPlayCommand = value; }
        }
        #endregion

    }
}
