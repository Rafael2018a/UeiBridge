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
using System.Windows.Data;

namespace ByteStreamer3
{
    class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        #region === privates ======
        DirectoryInfo _playFolder;
        bool _nowPlayingFlag;
        string _filesToPlayMessage;
        string _playFolderBoxBorderColor;
        string _settingsFilename = "bytestreamer3.setting.bin";
        SettingBag _settingBag;
        #endregion
        #region ==== Prop's =======
        public bool IsPlayOneByOne { get; set; }
        public bool IsRepeat { get; set; }
        public PacketPlayerViewModel PlayerViewModel { get; private set; }
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
        public string FilesToPlayMessage 
        { 
            get => _filesToPlayMessage;
            set
            {
                _filesToPlayMessage = value;
                RaisePropertyChangedEvent("FilesToPlayMessage");
            }
        }
        public string PlayFolderBoxBorderColor
        {
            get => _playFolderBoxBorderColor;
            set
            {
                _playFolderBoxBorderColor = value;
                RaisePropertyChangedEvent("PlayFolderBoxBorderColor");
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
        void StartPlay(object obj)
        {
            PlayerViewModel.StartPlay();
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
                _settingBag.PlayFolder = PlayFolder;
                PlayerViewModel.SetPlayFolder(new DirectoryInfo(PlayFolder));
                PlayFolderBoxBorderColor = (_playFolder.Exists) ? "Gray" : "Red";
                
            }
        }
        bool CanSelectPlayFolder(object obj)
        {
            return true;
        }
        #endregion

        //PacketPlayerViewModel _playerViewModel;
        public MainViewModel()
        {
            LoadCommands();
            _settingBag = LoadSetting();

            _playFolder = new DirectoryInfo(_settingBag.PlayFolder);
            // start update gui task
            System.Windows.Threading.DispatcherTimer timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(3);
            timer.Tick += UpateGui_timeCallback;
            timer.Start();

            PlayFolderBoxBorderColor = (_playFolder.Exists) ? "Gray" : "Red";
            IsPlayOneByOne = true; // tbd
            PlayerViewModel = new PacketPlayerViewModel(_playFolder, IsRepeat, IsPlayOneByOne);

            //_packetPlayer = new PacketPlayer(_playFolder, _repeatFlag, _playOneByOneFlag);
            //PlayList = new ObservableCollection<PlayItemViewModel>(_packetPlayer.PlayList.Select(i => new PlayItemViewModel(i)));
        }
        ~MainViewModel()
        {
            SaveSetting(_settingBag);
        }

        private void UpateGui_timeCallback(object sender, EventArgs e)
        {
            
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
        private SettingBag LoadSetting()
        {
            System.Runtime.Serialization.IFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            SettingBag sb;
            try
            {
                System.IO.FileStream fs = new System.IO.FileStream(_settingsFilename, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                object o = formatter.Deserialize(fs);
                sb = o as SettingBag;
            }
            catch (Exception ex)
            {
                sb = new SettingBag();
            }

            //if (o != null)
            //    return sb;
            //else
            return sb;
        }

        void SaveSetting(SettingBag setting)
        {
            System.Runtime.Serialization.IFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            System.IO.FileStream fs = new System.IO.FileStream(_settingsFilename, System.IO.FileMode.Create, System.IO.FileAccess.Write);
            formatter.Serialize(fs, setting);
            fs.Close();
        }



    }
}
