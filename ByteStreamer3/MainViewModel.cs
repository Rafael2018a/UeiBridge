using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using ByteStreamer3.Utilities;
using System.Collections.ObjectModel;

namespace ByteStreamer3
{
    class MainViewModel : System.ComponentModel.INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        #region === privates ======
        string _destinationAddress = "10.10.10.10";
        DirectoryInfo _playFolder = new DirectoryInfo(@".");

        ObservableCollection<PlayItem> _nowPlayingList = new ObservableCollection<PlayItem>();
        bool _nowPlayingFlag;
        bool _repeatFlag;
        bool _playOneByOneFlag;
        PacketPlayer _packetPlayer;
        #endregion

        #region ==== Prop's =======
        public bool PlayOneByOneFlag { get => _playOneByOneFlag; set => _playOneByOneFlag = value; }
        public bool RepeatFlag { get => _repeatFlag; set => _repeatFlag = value; }
        public ObservableCollection<PlayItem> NowPlayingList => _nowPlayingList;
        public DirectoryInfo PlayFolder { get => _playFolder; set => _playFolder=value; }
        public bool NowPlayingFlag
        {
            get => _nowPlayingFlag;
            set
            {
                _nowPlayingFlag = value;
                RaisePropertyChangedEvent("NowPlayingFlag");
            }
        }
        public string DestinationAddress
        {
            get => _destinationAddress;
            set
            {
                _destinationAddress = value;
                RaisePropertyChangedEvent("DestinationAddress");
            }
        }
        #endregion

        #region ==== Commands =====
        RelayCommand _startPlayCommand;
        public Utilities.RelayCommand StartPlayCommand { get => _startPlayCommand; set => _startPlayCommand = value; }
        Utilities.RelayCommand _browseFolderCommand;
        RelayCommand _stopPlayCommand;
        public RelayCommand StopPlayCommand { get => _stopPlayCommand; set => _stopPlayCommand = value; }
        public RelayCommand BrowseFolderCommand { get => _browseFolderCommand; set => _browseFolderCommand = value; }
        #endregion

        public MainViewModel()
        {
            LoadCommands();

            // start update gui task
            System.Windows.Threading.DispatcherTimer timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(3);
            timer.Tick += UpateGui_timeCallback;
            timer.Start();

            _nowPlayingList.Add(new PlayItem() { Name = "File1", PlayedBlocks = 425 });
        }

        private void UpateGui_timeCallback(object sender, EventArgs e)
        {
            _nowPlayingList.Add(new PlayItem() { Name = "File1", PlayedBlocks = 425 });

        }

        void RaisePropertyChangedEvent(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
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

        void StartPlay(object obj)
        {
            _packetPlayer = new PacketPlayer(_playFolder, _repeatFlag, _playOneByOneFlag);
            _packetPlayer.StartPlay();
        }
        bool CanStartPlay(object obj)
        {
            return (NowPlayingFlag == false);
        }

    }
}
