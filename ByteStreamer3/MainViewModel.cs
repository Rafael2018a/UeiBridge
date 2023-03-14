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
        //string _filesToPlayMessage;
        string _playFolderBoxBorderColor;
        const string _settingsFilename = "bytestreamer3.setting.bin";
        SettingBag _settingBag;
        int _simpleCounter;
        System.Windows.Threading.DispatcherTimer _guiUpdateTimer = new System.Windows.Threading.DispatcherTimer();
        bool _nowPlaying = false;
        System.Windows.Window _parentWindow;
        ObservableCollection<PlayFileViewModel> _playList;
        System.Threading.CancellationTokenSource _playCancelSource;
        #endregion

        #region ==== Publics =======
        public bool IsPlayOneByOne { get; set; }
        public bool IsRepeat { get; set; }
        //public FilePlayer2 Player { get; private set; }
        public string PlayFolder
        {
            get { return _playFolder.FullName; }
            set
            {
                _playFolder = new DirectoryInfo(value);
                RaisePropertyChangedEvent("PlayFolder");
            }
        }
        //public string FilesToPlayMessage 
        //{ 
        //    get => _filesToPlayMessage;
        //    set
        //    {
        //        _filesToPlayMessage = value;
        //        RaisePropertyChangedEvent("FilesToPlayMessage");
        //    }
        //}
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
        public bool NowPlaying
        {
            get => _nowPlaying;
            set
            {
                _nowPlaying = value;
                _parentWindow.Dispatcher.Invoke(() =>
                {
                    StartPlayCommand.OnCanExecuteChanged();
                    StopPlayCommand.OnCanExecuteChanged();
                    BrowseFolderCommand.OnCanExecuteChanged();
                });
            }
        }
        public ObservableCollection<PlayFileViewModel> PlayList
        {
            get => _playList;
            set
            {
                _playList = value;
                RaisePropertyChangedEvent("PlayList");
            }
        }

        #endregion

        #region ==== Commands =====
        public RelayCommand StartPlayCommand { get; set; }
        public RelayCommand StopPlayCommand { get; set; }
        public RelayCommand BrowseFolderCommand { get; set; }
        void StartPlay(object obj)
        {
            _guiUpdateTimer.Interval = TimeSpan.FromMilliseconds(1000);
            _guiUpdateTimer.Tick += OnGuiUpdateTimeTick;
            _guiUpdateTimer.Start();

            NowPlaying = true;
            if (IsPlayOneByOne)
            {
                StartPlayOneByOne(_playList.ToList());
            }
            else
            {
                StartPlaySimultaneously(_playList.ToList());
            }
        }
        bool CanStartPlay(object obj)
        {
            return (_nowPlaying == false);
        }
        void StopPlay(object obj)
        {
            _playCancelSource.Cancel();
            //Player.StopPlay();
            NowPlaying = false;
        }
        bool CanStopPlay(object obj)
        {
            return _nowPlaying;
        }
        void SelectPlayFolder(object obj)
        {
            // prepare & show dialog
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            dialog.RestoreDirectory = true;
            dialog.AddToMostRecentlyUsedList = true;
            dialog.InitialDirectory = PlayFolder;
            CommonFileDialogResult result = dialog.ShowDialog();
            // 
            if (result == CommonFileDialogResult.Ok)
            {
                PlayFolder = dialog.FileName;
                _settingBag.PlayFolder = PlayFolder;
                SetPlayFolder(new DirectoryInfo(PlayFolder));
                PlayFolderBoxBorderColor = (_playFolder.Exists) ? "Gray" : "Red";
            }
        }
        bool CanSelectPlayFolder(object obj)
        {
            return _nowPlaying == false;
        }
        private void LoadCommands()
        {
            StartPlayCommand = new RelayCommand(StartPlay, CanStartPlay);
            StopPlayCommand = new RelayCommand(StopPlay, CanStopPlay);
            BrowseFolderCommand = new RelayCommand(SelectPlayFolder, CanSelectPlayFolder);
        }
        #endregion

        void SetPlayFolder(DirectoryInfo playFolder)
        {
            if (!playFolder.Exists)
                return;
            this._playFolder = playFolder;
            FileInfo[] jsonlist = playFolder.GetFiles("*.json");
            var onlyValids = new List<PlayFile>(jsonlist.Select(i => new PlayFile(i)).Where(i => i.IsValidItem()));
            var vmlist = onlyValids.Select(i => new PlayFileViewModel(i));
            PlayList = new ObservableCollection<PlayFileViewModel>(vmlist);

            // create sample file
            if (onlyValids.Count == 0)
            {
                JFileClass jclass = new JFileClass(new JFileHeader(), new JFileBody(new int[] { 01, 02, 03 }));
                string s = Newtonsoft.Json.JsonConvert.SerializeObject(jclass, Formatting.Indented);
                using (StreamWriter fs = new StreamWriter("sample.json"))
                {
                    fs.Write(s);
                }
            }
        }
        public MainViewModel(System.Windows.Window parentWindow)
        {
            LoadCommands();
            _settingBag = LoadSetting();
            _parentWindow = parentWindow;
            _playFolder = new DirectoryInfo(_settingBag.PlayFolder);
            SetPlayFolder(_playFolder);
            PlayFolderBoxBorderColor = (_playFolder.Exists) ? "Gray" : "Red";
            // defaults
            IsPlayOneByOne = true;
            IsRepeat = false;

            var v = System.Reflection.Assembly.GetEntryAssembly().GetName().Version;
            parentWindow.Title = "ByteStreamer " + v.ToString(3);
        }
        ~MainViewModel()
        {
            // Save Setting 
            System.Runtime.Serialization.IFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            using (FileStream fs = new FileStream(_settingsFilename, FileMode.Create, System.IO.FileAccess.Write))
            {
                formatter.Serialize(fs, _settingBag);
            }
        }

        private void OnGuiUpdateTimeTick(object sender, EventArgs e)
        {
            //++SimpleCounter;
        }

        void RaisePropertyChangedEvent(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
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

            return sb;
        }
        private Task StartPlayOneByOne(List<PlayFileViewModel> playList)
        {
            _playCancelSource = new System.Threading.CancellationTokenSource();
            Task t = Task.Factory.StartNew(() =>
            {
                try
                {
                    do
                    {
                        foreach (PlayFileViewModel playfileVM in playList)
                        {
                            if ((!playfileVM.PlayFile.IsValidItem()) || (false == playfileVM.IsItemChecked))
                                continue;

                            playfileVM.SetPlayedBlocksCount(0);
                            for (int i = 0; i < playfileVM.PlayFile.JFileObject.Header.NumberOfCycles; i++)
                            {
                                // -- send block ....
                                playfileVM.PlayFile.SendBlockByUdp();
                                System.Threading.Thread.Sleep(playfileVM.PlayFile.JFileObject.Header.WaitStateMs);
                                playfileVM.SetPlayedBlocksCount(1 + playfileVM.PlayedBlocksCount);

                                _playCancelSource.Token.ThrowIfCancellationRequested();
                            }
                        }
                    } while (IsRepeat && (false == _playCancelSource.Token.IsCancellationRequested));
                }
                finally
                {
                    NowPlaying = false;
                    _playCancelSource.Dispose();
                }
            }, _playCancelSource.Token);

            return t;
        }
        /// <summary>
        /// Start Play Simultaneously
        /// </summary>
        private Task StartPlaySimultaneously(List<PlayFileViewModel> playList)
        {
            NowPlaying = true;
            _playCancelSource = new System.Threading.CancellationTokenSource();
            List<Task> playTaskList = new List<Task>();

                foreach (PlayFileViewModel playfileVM in playList)
                {
                    if ((!playfileVM.PlayFile.IsValidItem()) || (false == playfileVM.IsItemChecked))
                        continue;

                    Task t = Task.Run(() =>
                       {
                           playfileVM.SetPlayedBlocksCount(0);
                           for (int i = 0; i < playfileVM.PlayFile.JFileObject.Header.NumberOfCycles; i++)
                           {
                               playfileVM.PlayFile.SendBlockByUdp();
                               System.Threading.Thread.Sleep(playfileVM.PlayFile.JFileObject.Header.WaitStateMs);
                               playfileVM.SetPlayedBlocksCount(1 + playfileVM.PlayedBlocksCount);

                               _playCancelSource.Token.ThrowIfCancellationRequested();
                           }
                       }, _playCancelSource.Token);
                    playTaskList.Add(t);
                }
            

            Task result = Task.Factory.ContinueWhenAll(playTaskList.ToArray(), z => NowPlaying = false);
            return result;
        }
    }
}
