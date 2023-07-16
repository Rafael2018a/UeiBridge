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
    class MainViewModel : ViewModelBase
    {
        //public event PropertyChangedEventHandler PropertyChanged;

        #region === privates ======
        private string _playFolderBoxBorderColor;
        //private const string _settingsFilename = "bytestreamer3.setting.bin";
        private const string _statePersistFilename = "ByteStreamerState.json";
        //private SettingBag _settingBag;
        private AppState _appState;
        private int _simpleCounter;
        private System.Windows.Threading.DispatcherTimer _guiUpdateTimer = new System.Windows.Threading.DispatcherTimer();
        private bool _nowPlaying = false;
        private System.Windows.Window _parentWindow;
        private ObservableCollection<PlayFileViewModel> _playFileVMList;
        private System.Threading.CancellationTokenSource _playCancelSource;
        private string _playFolderString;
        #endregion

        #region ==== Publics =======
        public bool IsPlayOneByOne { get; set; }
        public bool IsRepeat { get; set; }
        //public FilePlayer2 Player { get; private set; }
        public string PlayFolderString
        {
            get { return _playFolderString; }
            set
            {
                _playFolderString = value;
                //RaisePropertyChangedEvent("PlayFolderString");
                RaisePropertyChanged();
            }
        }
        public string PlayFolderBoxBorderColor
        {
            get => _playFolderBoxBorderColor;
            set
            {
                _playFolderBoxBorderColor = value;
                //RaisePropertyChangedEvent("PlayFolderBoxBorderColor");
                RaisePropertyChanged();
            }
        }
        public int SimpleCounter
        {
            get => _simpleCounter;
            set
            {
                _simpleCounter = value;
                //RaisePropertyChangedEvent("SimpleCounter");
                RaisePropertyChanged();
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
                    ReloadFilesCommand.OnCanExecuteChanged();
                });
            }
        }
        public ObservableCollection<PlayFileViewModel> PlayFileVMList
        {
            get => _playFileVMList;
            set
            {
                _playFileVMList = value;
                //RaisePropertyChangedEvent("PlayFileVMList");
                RaisePropertyChanged();
            }
        }

        #endregion

        #region ==== Commands =====
        public RelayCommand StartPlayCommand { get; set; }
        public RelayCommand StopPlayCommand { get; set; }
        public RelayCommand BrowseFolderCommand { get; set; }
        public RelayCommand ReloadFilesCommand { get; set; }
        void StartPlay(object obj)
        {
            _guiUpdateTimer.Interval = TimeSpan.FromMilliseconds(1000);
            _guiUpdateTimer.Tick += OnGuiUpdateTimeTick;
            _guiUpdateTimer.Start();

            NowPlaying = true;
            foreach (var i in _playFileVMList)
            {
                i.PlayedBlocksCount = 0;
            }
            if (IsPlayOneByOne)
            {
                StartPlayOneByOne(_playFileVMList.ToList());
            }
            else
            {
                StartPlaySimultaneously(_playFileVMList.ToList());
            }
        }
        bool CanStartPlay(object obj)
        {
            return (_nowPlaying == false);
        }
        void ReloadFiles(object obj)
        {
            //var dirInfo = new DirectoryInfo(_settingCache.PlayFolder);
            //_appState = LoadStateFile(_statePersistFilename);
            UpdatePlayList(_appState);
        }
        bool CanReloadFiles(object obj)
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
            dialog.InitialDirectory = PlayFolderString;
            CommonFileDialogResult result = dialog.ShowDialog();
            // 
            if (result == CommonFileDialogResult.Ok)
            {
                PlayFolderString = dialog.FileName;
                _appState.PlayFolder = PlayFolderString;
                UpdatePlayList(_appState);
                var di = new DirectoryInfo(PlayFolderString);
                PlayFolderBoxBorderColor = (di.Exists) ? "Gray" : "Red";
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
            ReloadFilesCommand = new RelayCommand(ReloadFiles, CanReloadFiles);
        }
        #endregion



        private static void CreateSampleFile(string playFolder)
        {
            string filename = "sample.json";
            DirectoryInfo folderInfo = new DirectoryInfo(playFolder);
            var first = folderInfo.GetFiles(filename).FirstOrDefault();
            // create sample file
            if (first == null)
            {
                JFileClass jclass = new JFileClass(new JFileHeader(), new JFileBody(new int[] { 01, 02, 03 }));
                string s = JsonConvert.SerializeObject(jclass, Formatting.Indented);
                string fn = Path.Combine( folderInfo.FullName, filename);
                using (StreamWriter fs = new StreamWriter(fn))
                {
                    fs.Write(s);
                }
            }
        }

        AppState LoadStateFile(string filename)
        {
            // load persist file
            FileInfo stateFile = new FileInfo(_statePersistFilename);
            AppState jsp = new AppState( "."); // empty state object
            if (stateFile.Exists)
            {
                using (StreamReader reader = stateFile.OpenText())
                {
                    jsp = JsonConvert.DeserializeObject<AppState>(reader.ReadToEnd());
                }
                if (null==jsp.PlayFolder)
                {
                    jsp.PlayFolder = ".";
                }
            }
            return jsp;
        }


        public MainViewModel(System.Windows.Window parentWindow)
        {
            LoadCommands();
            //var dirInfo = new DirectoryInfo(_settingCache.PlayFolder);
            _appState = LoadStateFile( _statePersistFilename);//LoadSetting();
            UpdatePlayList(_appState);

            _parentWindow = parentWindow;

            PlayFolderString = _appState.PlayFolder;

            if (_appState.EntryStateList.Count == 0)
            {
                CreateSampleFile(_appState.PlayFolder);
            }
            PlayFolderBoxBorderColor = Directory.Exists( _appState.PlayFolder) ? "Gray" : "Red";
            // defaults
            IsPlayOneByOne = true;
            IsRepeat = false;

            var v = System.Reflection.Assembly.GetEntryAssembly().GetName().Version;
            parentWindow.Title = "ByteStreamer " + v.ToString(3);
        }
        //~MainViewModel()
        //{
        //    // Save Setting 
        //    System.Runtime.Serialization.IFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
        //    using (FileStream fs = new FileStream(_settingsFilename, FileMode.Create, System.IO.FileAccess.Write))
        //    {
        //        formatter.Serialize(fs, _settingCache);
        //    }
        //    foreach( var f in PlayFileVMList)
        //    {
        //        f.PlayFile.Save();
        //    }
        //}
        private void UpdatePlayList(AppState jsp)
        {
            if (jsp.PlayFolder==null)
            {
                return;
            }
            var playFolderInfo = new DirectoryInfo(jsp.PlayFolder);
            //var di = new DirectoryInfo(jsp.PlayFolder);
            if (playFolderInfo.Exists)
            {
                FileInfo[] jsonFilesInFolder = playFolderInfo.GetFiles("*.json");

                // remove non-valid files and add 'isChecked'
                List<JFileAux> validJsonFilesInFolder = jsonFilesInFolder.Select((FileInfo i) => new JFileAux(i)).Where((JFileAux i) => i.IsValidItem()).ToList();
                foreach (JFileAux validJsonFile in validJsonFilesInFolder)
                {
                    var u = jsp.GetEntryState(validJsonFile.PlayFileInfo);
                    if (null != u)
                    {
                        validJsonFile.JFileObject.Header.EnablePlay = u.IsChecked;
                    }
                }

                // project
                var vmlist = validJsonFilesInFolder.Select(i => new PlayFileViewModel(i));
                PlayFileVMList = new ObservableCollection<PlayFileViewModel>(vmlist);
            }
        }

        private void OnGuiUpdateTimeTick(object sender, EventArgs e)
        {
            //++SimpleCounter;
        }

        //void RaisePropertyChangedEvent(string propName)
        //{
        //    if (PropertyChanged != null)
        //        PropertyChanged(this, new PropertyChangedEventArgs(propName));
        //}
        private int _radBtnId = 1;
        public int IsSuccess
        {
            get
            { return _radBtnId; }
            set
            { _radBtnId = value; }
        }

        //private SettingBag LoadSetting()
        //{
        //    System.Runtime.Serialization.IFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
        //    SettingBag sb;
        //    try
        //    {
        //        System.IO.FileStream fs = new System.IO.FileStream(_settingsFilename, System.IO.FileMode.Open, System.IO.FileAccess.Read);
        //        object o = formatter.Deserialize(fs);
        //        sb = o as SettingBag;
        //    }
        //    catch (FileNotFoundException)
        //    {
        //        sb = new SettingBag();
        //    }
        //    return sb;
        //}
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

                            var ip = System.Net.IPAddress.Parse(playfileVM.PlayFile.JFileObject.Header.DestIp);
                            var destEp = new System.Net.IPEndPoint(ip, playfileVM.PlayFile.JFileObject.Header.DestPort);
                            var udpWriter = new UdpWriter(destEp);

                            playfileVM.PlayedBlocksCount = 0;
                            for (int i = 0; i < playfileVM.PlayFile.JFileObject.Header.NumberOfCycles; i++)
                            {
                                // -- send block ....
                                var eth = JFileAux.JsonToEtherentMessage(playfileVM.PlayFile.JFileObject);
                                udpWriter.Send(eth.GetByteArray(UeiBridge.Library.MessageWay.downstream));

                                System.Threading.Thread.Sleep(playfileVM.PlayFile.JFileObject.Header.WaitStateMs);
                                playfileVM.PlayedBlocksCount++;

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

                Task t = Task.Factory.StartNew(() =>
                   {
                       var ip = System.Net.IPAddress.Parse(playfileVM.PlayFile.JFileObject.Header.DestIp);
                       var destEp = new System.Net.IPEndPoint(ip, playfileVM.PlayFile.JFileObject.Header.DestPort);
                       var udpWriter = new UdpWriter(destEp);

                       playfileVM.PlayedBlocksCount = 0;
                       for (int i = 0; i < playfileVM.PlayFile.JFileObject.Header.NumberOfCycles; i++)
                       {
                           var eth = JFileAux.JsonToEtherentMessage(playfileVM.PlayFile.JFileObject);
                           udpWriter.Send(eth.GetByteArray(UeiBridge.Library.MessageWay.downstream));
                           System.Threading.Thread.Sleep(playfileVM.PlayFile.JFileObject.Header.WaitStateMs);
                           playfileVM.PlayedBlocksCount++;

                           _playCancelSource.Token.ThrowIfCancellationRequested();
                       }
                   }, _playCancelSource.Token);

                playTaskList.Add(t);
            }

            Task result = Task.Factory.ContinueWhenAll(playTaskList.ToArray(), z => NowPlaying = false);
            return result;
        }

        internal void OnClosing()
        {
            // Save Setting 
            //System.Runtime.Serialization.IFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            //using (FileStream fs = new FileStream(_settingsFilename, FileMode.Create, System.IO.FileAccess.Write))
            //{
            //    formatter.Serialize(fs, _settingBag);
            //}

            // build persist-list
            AppState jsp = new AppState(_appState.PlayFolder);
            foreach (var f in PlayFileVMList)
            {
                jsp.EntryStateList.Add(new FileState(f.IsItemChecked, f.Filename));
            }
            // save persist list
            string s = JsonConvert.SerializeObject(jsp, Formatting.Indented);
            using (StreamWriter fs = new StreamWriter(_statePersistFilename))
            {
                fs.Write(s);
            }

        }
    }
}
