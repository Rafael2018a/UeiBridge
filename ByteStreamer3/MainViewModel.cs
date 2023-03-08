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
        string _filesToPlayMessage;
        string _playFolderBoxBorderColor;
        string _settingsFilename = "bytestreamer3.setting.bin";
        SettingBag _settingBag;
        int _simpleCounter;
        System.Windows.Threading.DispatcherTimer _guiUpdateTimer = new System.Windows.Threading.DispatcherTimer();
        bool _nowPlaying = false;
        #endregion

        #region ==== Prop's =======
        public bool IsPlayOneByOne { get; set; }
        public bool IsRepeat { get; set; }
        public PacketPlayer2 Player { get; private set; }
        public string PlayFolder 
        {
            get { return _playFolder.FullName; }
            set 
            { 
                _playFolder = new DirectoryInfo(value);
                RaisePropertyChangedEvent("PlayFolder");
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
        public int SimpleCounter
        {
            get => _simpleCounter;
            set
            {
                _simpleCounter = value;
                RaisePropertyChangedEvent("SimpleCounter");
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
            _guiUpdateTimer.Interval = TimeSpan.FromMilliseconds(1000);
            _guiUpdateTimer.Tick += OnGuiUpdateTimeTick;
            _guiUpdateTimer.Start();

            _nowPlaying = true;
            Player.StartPlay().ContinueWith(t => { _nowPlaying = false; _mw.Dispatcher.Invoke(() => StartPlayCommand.OnCanExecuteChanged()); });
            //Task.Factory.StartNew(() =>
            //{
            //    _nowPlaying = true;
            //    System.Threading.Thread.Sleep(2000);
            //    _nowPlaying = false;


            //}).ContinueWith(t => { _mw.Dispatcher.Invoke(() => StartPlayCommand.OnCanExecuteChanged()); });
        }

        private void StartPlayCommand_CanExecuteChanged(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        bool CanStartPlay(object obj)
        {
            return (_nowPlaying == false);
            //return ((Player==null) || (Player.NowPlaying == false));
        }
        void StopPlay(object obj)
        {
            Player.StopPlay();
        }
        bool CanStopPlay(object obj)
        {
            return (Player?.NowPlaying==true);
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
                Player.SetPlayFolder(new DirectoryInfo(PlayFolder));
                PlayFolderBoxBorderColor = (_playFolder.Exists) ? "Gray" : "Red";
            }
        }
        bool CanSelectPlayFolder(object obj)
        {
            return true;
        }
        #endregion

        MainWindow _mw;

        public MainViewModel( MainWindow mw)
        {
            LoadCommands();
            _settingBag = LoadSetting();

            _mw = mw;

            _playFolder = new DirectoryInfo(_settingBag.PlayFolder);
            // start update gui task
            
            PlayFolderBoxBorderColor = (_playFolder.Exists) ? "Gray" : "Red";
            IsPlayOneByOne = true; // tbd

            Player = new PacketPlayer2(_playFolder, IsRepeat, IsPlayOneByOne);
            //_packetPlayer = new PacketPlayer(_playFolder, _repeatFlag, _playOneByOneFlag);
            //PlayList = new ObservableCollection<PlayItemViewModel>(_packetPlayer.PlayList.Select(i => new PlayItemViewModel(i)));
        }
        ~MainViewModel()
        {
            SaveSetting(_settingBag);
        }

        private void OnGuiUpdateTimeTick(object sender, EventArgs e)
        {
            ++SimpleCounter;
        }

        void RaisePropertyChangedEvent(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
        private void LoadCommands()
        {
            StartPlayCommand = new RelayCommand(StartPlay, CanStartPlay);
            StopPlayCommand = new Utilities.RelayCommand(StopPlay, CanStopPlay);
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
            catch (FileNotFoundException)
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
