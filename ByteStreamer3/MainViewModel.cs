using ByteStreamer3.Utilities;
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

            //List<User> items = new List<User>();
            _nowPlayingList.Add(new User() { Name = "John Doe", Age = 42, Mail = "h1@g.mail" });
            _nowPlayingList.Add(new User() { Name = "Jane Doe", Age = 39 });
            _nowPlayingList.Add(new User() { Name = "Sammy Doe", Age = 13 });

        }
        List<User> _nowPlayingList = new List<User>();
        public List<User> NowPlayingList => _nowPlayingList;
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
        string PlayFolder { get; set; }
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

        bool _nowPlayingFlag;
        public bool NowPlayingFlag
        {
            get => _nowPlayingFlag;
            set
            {
                _nowPlayingFlag = value;
                RaisePropertyChangedEvent("NowPlayingFlag");
            }
        }

        void StartPlay(object obj)
        {
            System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(PlayFolder);
            PacketPlayer pp = new PacketPlayer(di);
            pp.Start();
        }
        bool CanStartPlay(object obj)
        {
            return (NowPlayingFlag == false);
        }

        #region ICommands =====
        Utilities.RelayCommand _startPlayCommand;
        public Utilities.RelayCommand StartPlayCommand { get => _startPlayCommand; set => _startPlayCommand = value;
        }
        Utilities.RelayCommand _browseFolderCommand;
        //BrowseFolderCommand
        private Utilities.RelayCommand _stopPlayCommand;
        public Utilities.RelayCommand StopPlayCommand
        {
            get { return _stopPlayCommand; }
            set { _stopPlayCommand = value; }
        }

        public RelayCommand BrowseFolderCommand { get => _browseFolderCommand; set => _browseFolderCommand = value; }
        bool _isRepeatFlag;
        public bool IsRepeatFlag { get => _isRepeatFlag; set => _isRepeatFlag = value; }
        bool _isPlayOneByOne;
        public bool IsPlayOneByOne { get => _isPlayOneByOne; set => _isPlayOneByOne = value; }
        #endregion

    }
}
