// Ignore Spelling: Uei

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Globalization;
using UeiBridge.Library;
using System.Windows;
using System.Collections.ObjectModel;
using Microsoft.WindowsAPICodePack.Dialogs;
using Microsoft.Win32;

namespace UeiBridge.CubeNet
{
    class MainViewModel : ViewModelBase
    {
        #region == commands ==
        public RelayCommand CreateEmptyRepositoryCommand { get; set; }
        public RelayCommand GetFreeIpCommand { get; set; }
        public RelayCommand GetCubeSignatureCommand { get; set; }
        
        public RelayCommand AddCubeToRepositoryCommand { get; set; }
        //public RelayCommand AddCubeToExistingEntryCommand { get; set; }
        public RelayCommand SaveRepositoryCommand { get; set; }
        public RelayCommand AcceptAddressCommand { get; set; }
        public RelayCommand ResetPaneCommand { get; set; }
        public RelayCommand ExitAppCommand { get; set; }
        public RelayCommand PickRepoFileCommand { get; set; }
        public RelayCommand CloseRepositoryCommand { get; set; }
        public RelayCommand GetRepositoryEntriesCommand { get; set; }
        public RelayCommand GenerateSetupFileCommand { get; set; }
        #endregion
        #region == privates ==
        string _cubeSignature;
        IPAddress _cubeAddress;
        //string _cubeNickname;
        //string _cubeDesc;
        string _messageToUser;
        //FileInfo _repositoryFile;
        const string _repositoryFileName = "CubeRepository.json";
        CubeRepositoryProxy _repositoryProxy = new CubeRepositoryProxy();
        string _panelLogMessage;
        private bool _isAddressEnabled=true;
        bool _canAddCubeToRepository = false;
        bool _canGetCubeSignature = false;
        bool _canGetFreeIp = true;
        private System.Windows.Window _parentWindow;
        FileInfo _repositoryFileInfo;
        string _repoStat;
        #endregion
        #region == publics ==
        public string CubeSignature
        {
            get => _cubeSignature;
            set
            {
                _cubeSignature = value;
                RaisePropertyChanged();
            }
        }
        public string MessageToUser
        {
            get => _messageToUser;
            set
            {
                _messageToUser = value;
                RaisePropertyChanged();
            }
        }
        public IPAddress CubeAddress
        {
            get => _cubeAddress;
            set
            {
                _cubeAddress = value;
                RaisePropertyChanged();
            }
        }
        //List<string> _cubeTypeList;
        public string PanelLogMessage { get => _panelLogMessage; set { _panelLogMessage = value; RaisePropertyChanged(); } }
        public string PanelLogToolTip { get; set; }
        public string CubeNickname { get; set; }
        public string CubeDesc { get; set; }


        private ObservableCollection<CubeType> _matchingCubeTypeList;
        private ObservableCollection<CubeType> _cubeTypeList;
        public bool IsAddressEnabled 
        { 
            get => _isAddressEnabled; 
            set 
            { 
                _isAddressEnabled = value; 
                RaisePropertyChanged(); 
            } 
        }

        public ObservableCollection<CubeType> MatchingCubeTypeList
        {
            get => _matchingCubeTypeList;
            set 
            { 
                _matchingCubeTypeList = value;
                RaisePropertyChanged();
            }
        }

        public CubeType SelectedCubeType { get; set; }

        bool _addAsNewCubeFlagValue;
        bool _addAsNewCubeFlagEnabled;

        public bool AddAsNewCubeFlagValue { get => _addAsNewCubeFlagValue; set { _addAsNewCubeFlagValue = value; RaisePropertyChanged(); } }
        public bool AddAsNewCubeFlagEnabled { get => _addAsNewCubeFlagEnabled; set { _addAsNewCubeFlagEnabled = value; RaisePropertyChanged(); } }

        public FileInfo RepositoryFileInfo
        {
            get => _repositoryFileInfo;
            set 
            {
                _repositoryFileInfo = value;
                RaisePropertyChanged();
            }
        }

        public string RepoStat
        {
            get => _repoStat;
            set
            {
                _repoStat = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<CubeType> CubeTypeList 
        { 
            get => _cubeTypeList;
            set
            {
                _cubeTypeList = value;
                RaisePropertyChanged();
            }
        }
        #endregion publics

        //IPAddress _cubeIp;
        public MainViewModel(System.Windows.Window parentWindow)
        {
            _parentWindow = parentWindow;
            LoadCommands();

            //var repFile = new FileInfo(_repositoryFileName);
            RepositoryFileInfo = new FileInfo(_repositoryFileName);
            //RepositoryFullName = "<no repository loaded>";
            if (RepositoryFileInfo.Exists)
            {
                try
                {
                    LoadRepositoryFile(RepositoryFileInfo);
                }
                catch (Exception ex)
                {
                    PanelLogMessage = $"Failed to load file {RepositoryFileInfo.FullName}. {ex.Message}";
                }
            }
            else
            {
                PanelLogMessage = $"Repository file {RepositoryFileInfo.FullName} doesn't exist.";
                RepositoryFileInfo = null;
            }
            //CubeTypeList = new ObservableCollection<string>();
            //CubeTypeList.Add("ct1");
            //CubeTypeList.Add("ct2");
        }

        private void LoadRepositoryFile(FileInfo repFile)
        {
            _repositoryProxy.LoadRepository(repFile);
            RepoStat = _repositoryProxy.GetRepoStatString();
            PanelLogMessage = $"Repository file {_repositoryProxy.RepositoryFile.Name} loaded";
            PanelLogToolTip = _repositoryProxy.RepositoryFile.FullName;
        }

        private void LoadCommands()
        {
            CreateEmptyRepositoryCommand = new RelayCommand(CreateEmptyRepository, CanCreateEmptyRepository);
            GetFreeIpCommand = new RelayCommand(GetFreeIp, CanGetFreeIp);
            AcceptAddressCommand = new RelayCommand(AcceptAddress, CanAcceptAddress);
            GetCubeSignatureCommand = new RelayCommand(GetCubeSignature, CanGetCubeSignature);
            AddCubeToRepositoryCommand = new RelayCommand(AddCubeToRepository, CanAddCubeToRepository);
            SaveRepositoryCommand = new RelayCommand( SaveRepository, CanSaveRepository);
            ResetPaneCommand = new RelayCommand(ResetPane, CanResetPane);
            ExitAppCommand = new RelayCommand(ExitApp);
            PickRepoFileCommand = new RelayCommand(PickRepoFile);
            CloseRepositoryCommand = new RelayCommand(CloseRepository, CanCloseRepository);
            GetRepositoryEntriesCommand = new RelayCommand(GetRepositoryEntries, CanGetRepositoryEntries);
            GenerateSetupFileCommand = new RelayCommand(GenerateSetupFile, CanGenerateSetupFile);
        }

        private bool CanGenerateSetupFile(object obj)
        {
            return true;
        }

        private void GenerateSetupFile(object obj)
        {
            CubeType ct = obj as CubeType;
            if (ct == null)
            {
                return;
            }

            if (null!=ct.CubeSignature)
            {
                List<UeiDeviceInfo> devInfoList = new List<UeiDeviceInfo>();
                // populate list
                string[] deviceList = ct.CubeSignature.Split('/');
                int slot = 0;
                foreach(string dev in deviceList)
                {
                    devInfoList.Add(new UeiDeviceInfo("-", slot++, dev));
                }

                CubeSetup cs = new CubeSetup(devInfoList);

                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.FileName = CubeSetup.GetSelfFilename(ct.NickName);
                if (saveFileDialog.ShowDialog() == true)
                {
                    cs.AssociatedFileFullname = saveFileDialog.FileName;
                    cs.Serialize();
                    PanelLogMessage = $"File {cs.AssociatedFileFullname} saved";
                }
            }
            else
            {
                MessageBox.Show("Empty signature. Can't save file.", "Error", MessageBoxButton.OK);
            }
        }

        private bool CanGetRepositoryEntries(object obj)
        {
            return true;
        }

        private void GetRepositoryEntries(object obj)
        {
            var l = _repositoryProxy.GetCubeTypes();
            if (null != l)
            {
                CubeTypeList = new ObservableCollection<CubeType>(_repositoryProxy.GetCubeTypes());
            }
            else
            {
                CubeTypeList = null;
            }
        }

        private bool CanCloseRepository(object obj)
        {
            return true;
        }

        private void CloseRepository(object obj)
        {
            _repositoryProxy.CloseRepository();
            PanelLogMessage = $"Repository file {_repositoryProxy.RepositoryFile.Name} closed";
            PanelLogToolTip = _repositoryProxy.RepositoryFile.FullName;
            RepositoryFileInfo = null;
            RepoStat = null;
        }

        private void PickRepoFile(object obj)
        {

            var dialog = new CommonOpenFileDialog();
            //dialog.IsFolderPicker = true;
            dialog.RestoreDirectory = true;
            dialog.AddToMostRecentlyUsedList = true;
            dialog.EnsureFileExists = true;
           
            //dialog.InitialDirectory = PlayFolderString;
            CommonFileDialogResult result = dialog.ShowDialog();
            // 
            if (result == CommonFileDialogResult.Ok)
            {
                RepositoryFileInfo = new FileInfo(dialog.FileName);
                LoadRepositoryFile(RepositoryFileInfo);
            }
        }

        private void ExitApp(object obj)
        {
            //
            if (_repositoryProxy.CheckRepositoryChange())
            {
                MessageBoxResult result = MessageBox.Show("Repository not saved. Exit?", "Question", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.No)
                {
                    System.ComponentModel.CancelEventArgs e = obj as System.ComponentModel.CancelEventArgs;
                    e.Cancel = true;
                }
            }
        }

        private void ResetPane(object obj)
        {
            _canGetFreeIp = true;
            IsAddressEnabled = true;
            _canGetCubeSignature = false;
            CubeSignature = null;
            MatchingCubeTypeList?.Clear();
        }

        private bool CanResetPane(object obj)
        {
            return true;
        }

        private bool CanAcceptAddress(object obj)
        {
            return (null!=RepositoryFileInfo);
        }

        private void AcceptAddress(object obj)
        {
            if (null==CubeAddress)
            {
                return;
            }

            List<IPAddress> cubes = _repositoryProxy.GetAllPertainedCubes();
            bool ipExists = cubes.Any( i => i.Equals(CubeAddress));
            if (ipExists)
            {
                MessageBox.Show("ip already exists in repository", "Error", MessageBoxButton.OK);
                return;
            }
            IsAddressEnabled = false;
            MessageBox.Show("Please use PowerDNA explorer to change the physical cube address\nAfter that, click \"Get cube signature\"", "Cube IP", MessageBoxButton.OK);
            _canGetCubeSignature = true;
            CubeSignature = null;
            _canGetFreeIp = false;
        }

        //private bool CanAddCubeToExistingEntry1(object obj)
        //{
        //    return true;
        //}

        //private void AddCubeToExistingEntry1(object obj)
        //{
        //    var c = SelectedCubeType;
        //}

        private bool CanSaveRepository(object obj)
        {
            return true;
        }

        private void SaveRepository(object obj)
        {
            if (_repositoryProxy.IsRepositoryLoaded)
            {
                _repositoryProxy.SaveRepository();
            }
            
        }

        private bool CanCreateEmptyRepository(object obj)
        {
            return true;
        }

        private void CreateEmptyRepository(object obj)
        {
            RepositoryFileInfo = new FileInfo(_repositoryFileName);
            _repositoryProxy.CreateEmptyRepository(RepositoryFileInfo);
            if (true == _repositoryProxy.SaveRepository())
            {
                PanelLogMessage = $"Repository saved to file {RepositoryFileInfo.Name}.";
                //RepositoryFullName = repFile.FullName;
            }
        }

        private bool CanAddCubeToRepository(object obj)
        {
            return _canAddCubeToRepository;
        }

        private void AddCubeToRepository(object obj)
        {
            if (_repositoryProxy.IsRepositoryLoaded)
            {
                // add to existing
                if ((MatchingCubeTypeList.Count > 0) && (false == AddAsNewCubeFlagValue))
                {
                    if (null == SelectedCubeType)
                    {
                        MessageBox.Show("No entry selected", "Error", MessageBoxButton.OK);
                    }
                    else
                    {
                        SelectedCubeType.AddCube(CubeAddress);
                        PanelLogMessage = $"Cube {CubeAddress.ToString()} added to {SelectedCubeType.NickName}";
                        _canAddCubeToRepository = false;
                    }
                }
                else // add new cube type
                {
                    // add new cube-type
                    if ((null == CubeNickname) || (null == CubeDesc))
                    {
                        MessageBox.Show("Must fill both name and desc");
                        return;
                    }
                    if (_repositoryProxy.GetCubeTypeByNickName(CubeNickname).Count > 0)
                    {
                        MessageBox.Show("Nickname already exists");
                        return;
                    }

                    CubeType ct = _repositoryProxy.AddCubeType(CubeNickname, CubeDesc, CubeSignature);
                    ct.PertainCubeList.Add(CubeAddress.ToString());
                    PanelLogMessage = $"Cube {CubeAddress.ToString()} added as new entry - {CubeNickname}";
                    _canAddCubeToRepository = false;
                }
            }
            //_parentWindow.Dispatcher.Invoke(() =>
            //{
            //    AddCubeToRepositoryCommand.OnCanExecuteChanged();
            //});

        }

        private bool CanGetCubeSignature(object obj)
        {
            return _canGetCubeSignature;
        }

        private void GetCubeSignature(object obj)
        {
            if (null == _cubeAddress)
            {
                goto exit;
            }
            List<UeiDeviceInfo> devList = null;
            if (_cubeAddress.Equals(IPAddress.Any)) // ip 0.0.0.0 shall be identified as 'simu'
            {
                devList = CubeSeeker.GetDeviceList("simu://");
            }
            else if (null != CubeSeeker.TryIP(_cubeAddress)) // is cube connected?
            {
                devList = CubeSeeker.GetDeviceList(_cubeAddress);
            }
            else
            {
                MessageBox.Show($"Can't connect to cube {_cubeAddress.ToString()}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                CubeSignature = null;
                goto exit;
            }

            CubeSignature = BuildCubeSignature(devList);

            _canAddCubeToRepository = true;

            // fill cube list
            MatchingCubeTypeList = new ObservableCollection<CubeType>( _repositoryProxy.GetCubeTypesBySignature(CubeSignature));
            if (MatchingCubeTypeList.Count>0)
            {
                AddAsNewCubeFlagEnabled = true;
            }
            else
            {
                AddAsNewCubeFlagValue = true;
                AddAsNewCubeFlagEnabled = false;
            }

        exit: return;
        }

        private string BuildCubeSignature(List<UeiDeviceInfo> devList)
        {
            StringBuilder sb = new StringBuilder();
            foreach (UeiDeviceInfo udi in devList)
            {
                sb.Append(udi.DeviceName);
                sb.Append("/");
            }
            return sb.ToString();
        }


        private bool CanGetFreeIp(object obj)
        {
            return (_canGetFreeIp && RepositoryFileInfo!=null);
        }

        private void GetFreeIp(object obj)
        {
            if (_repositoryProxy.IsRepositoryLoaded)
            {
                _canGetCubeSignature = false;
                List<IPAddress> ipList = _repositoryProxy.GetAllPertainedCubes();
                List<byte> lsbList = ipList.Select(i => i.GetAddressBytes()[3]).ToList();
                //lsbList.Sort();
                int max = (lsbList.Count > 0) ? lsbList.Max() : 1;
                ++max;
                //byte last = lsbList[lsbList.Count - 1];
                IPAddress ipa = new IPAddress(new byte[] { 192, 168, 100, (byte)max });
                IsAddressEnabled = true;
                CubeAddress = ipa;
                //CubeSignature = null;
            }
            else
            {
                MessageBox.Show("No cube repository","", MessageBoxButton.OK);
            }
        }
    }
}
