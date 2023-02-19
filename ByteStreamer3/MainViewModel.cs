using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using ByteStreamer3.Utilities;
using System.Collections.ObjectModel;
using Microsoft.WindowsAPICodePack.Dialogs;
using Newtonsoft.Json;
using ByteStreame3.Utilities;
using System.Windows.Data;

namespace ByteStreamer3
{
    class MainViewModel : System.ComponentModel.INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        #region === privates ======
        string _destinationAddress = "10.10.10.10";
        DirectoryInfo _playFolder = new DirectoryInfo(@"c:\Users\Rafael\_gitRepos\UeiBridge.ByteStreamer3\ByteStreamer3\SampleJson\");

        ObservableCollection<PlayItemInfo> _nowPlayingList = new ObservableCollection<PlayItemInfo>();
        bool _nowPlayingFlag;
        bool _repeatFlag;
        bool _playOneByOneFlag;
        PacketPlayer _packetPlayer;
        string _filesToPlayMessage;
        //ObservableCollection<PlayItemInfo> _nowPlayingList;
        #endregion

        #region ==== Prop's =======
        public bool PlayOneByOneFlag { get => _playOneByOneFlag; set => _playOneByOneFlag = value; }
        public bool RepeatFlag { get => _repeatFlag; set => _repeatFlag = value; }
        public ObservableCollection<PlayItemInfo> NowPlayingList => _packetPlayer.NowPlayingList;
        public string PlayFolder 
        {
            get { return _playFolder.FullName; }
            set 
            { 
                _playFolder = new DirectoryInfo(value);
                RaisePropertyChangedEvent("PlayFolder");
            }
        }
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
        public string FilesToPlayMessage 
        { 
            get => _filesToPlayMessage;
            set
            {
                _filesToPlayMessage = value;
                RaisePropertyChangedEvent("FilesToPlayMessage");
            }
        }
        #endregion

        public MainViewModel()
        {
            LoadCommands();

            // start update gui task
            System.Windows.Threading.DispatcherTimer timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(3);
            timer.Tick += UpateGui_timeCallback;
            timer.Start();

            _packetPlayer = new PacketPlayer(_playFolder, _repeatFlag, _playOneByOneFlag);
            var n = _packetPlayer.NowPlayingList.Count();
            FilesToPlayMessage = $"{n} valid json files";
        }

        private void UpateGui_timeCallback(object sender, EventArgs e)
        {
            //_nowPlayingList.Add(new PlayItem() { Name = "File1", PlayedBlocks = 425 });

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
            BrowseFolderCommand = new RelayCommand(SelectPlayFolder, CanSelectPlayFolder);
        }
        private int _radBtnId = 1;
        public int IsSuccess
        {
            get
            { return _radBtnId; }
            set
            { _radBtnId = value; }
        }


        #region ==== Commands =====
        RelayCommand _startPlayCommand;
        public Utilities.RelayCommand StartPlayCommand { get => _startPlayCommand; set => _startPlayCommand = value; }
        Utilities.RelayCommand _browseFolderCommand;
        RelayCommand _stopPlayCommand;
        public RelayCommand StopPlayCommand { get => _stopPlayCommand; set => _stopPlayCommand = value; }
        public RelayCommand BrowseFolderCommand { get => _browseFolderCommand; set => _browseFolderCommand = value; }
        void StartPlay(object obj)
        {
            
            _packetPlayer.StartPlay();
        }
        bool CanStartPlay(object obj)
        {
            return (NowPlayingFlag == false);
        }

        void SelectPlayFolder(object obj)
        {
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            dialog.RestoreDirectory = true;
            dialog.AddToMostRecentlyUsedList = true;
            dialog.InitialDirectory = PlayFolder;
            CommonFileDialogResult result = dialog.ShowDialog();
            if (result == CommonFileDialogResult.Ok)
            {
                PlayFolder = dialog.FileName;
            }

            var n = _packetPlayer.NowPlayingList.Count;
            FilesToPlayMessage = $"{n} valid json files";
        }
        bool CanSelectPlayFolder(object obj)
        {
            return true;
        }

        #endregion

    }
}
