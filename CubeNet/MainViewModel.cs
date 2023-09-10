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

namespace UeiBridge.CubeNet
{
    class MainViewModel : ViewModelBase
    {
        #region == commands ==
        public RelayCommand CreateEmptyRepositoryCommand { get; set; }
        public RelayCommand GetFreeIpCommand { get; set; }
        public RelayCommand GetCubeSignatureCommand { get; set; }
        
        public RelayCommand AddCubeToNewEntryCommand { get; set; }
        public RelayCommand AddCubeToExistingEntryCommand { get; set; }
        public RelayCommand SaveRepositoryCommand { get; set; }
        public RelayCommand AcceptAddressCommand { get; set; }
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
        

        public ObservableCollection<string> CubeTypeList { get; set; }
        public bool IsAddressEnabled { 
            get => _isAddressEnabled; 
            set { 
                _isAddressEnabled = value; 
                RaisePropertyChanged(); 
            } }

        //public FileInfo RepositoryFile
        //{
        //    get => _repositoryFile;
        //    set
        //    {
        //        _repositoryFile = value;
        //        RaisePropertyChanged();
        //    }
        //}

        /// <summary>
        /// Indicates that repository exists
        /// </summary>
        //public string RepositoryFullName
        //{
        //    get => _repositoryFullName;
        //    set { _repositoryFullName = value; RaisePropertyChanged(); }
        //}
        #endregion

        //IPAddress _cubeIp;
        public MainViewModel()
        {
            LoadCommands();

            var repFile = new FileInfo(_repositoryFileName);
            //RepositoryFullName = "<no repository loaded>";
            if (repFile.Exists)
            {
                try
                {
                    _repositoryProxy.LoadRepository(repFile);
                    PanelLogMessage = $"Repository file {_repositoryProxy.RepositoryBackingFile.Name} loaded";
                    PanelLogToolTip = _repositoryProxy.RepositoryBackingFile.FullName;
                }
                catch (Exception ex)
                {
                    PanelLogMessage = $"Failed to load file {repFile.FullName}. {ex.Message}";
                }
            }
            else
            {
                PanelLogMessage = $"Repository file {repFile.FullName} doesn't exist.";
            }
            CubeTypeList = new ObservableCollection<string>();
            CubeTypeList.Add("ct1");
            CubeTypeList.Add("ct2");
        }
        private void LoadCommands()
        {
            CreateEmptyRepositoryCommand = new RelayCommand(CreateEmptyRepository, CanCreateEmptyRepository);
            GetFreeIpCommand = new RelayCommand(GetFreeIp, CanGetFreeIp);
            AcceptAddressCommand = new RelayCommand(AcceptAddress, CanAcceptAddress);
            GetCubeSignatureCommand = new RelayCommand(GetCubeSignature, CanGetCubeSignature);
            AddCubeToNewEntryCommand = new RelayCommand(AddCubeToNewEntry, CanAddCubeToNewEntry);
            AddCubeToExistingEntryCommand = new RelayCommand(AddCubeToExistingEntry, CanAddCubeToExistingEntry);
            SaveRepositoryCommand = new RelayCommand( SaveRepository, CanSaveRepository);
            
        }

        private bool CanAcceptAddress(object obj)
        {
            return true;
        }

        private void AcceptAddress(object obj)
        {
            List<IPAddress> cubes = _repositoryProxy.GetAllPertainedCubes();
            bool ipExists = cubes.Any( i => i.Equals(CubeAddress));
            if (ipExists)
            {
                MessageBox.Show("ip already exists in repository", "Error", MessageBoxButton.OK);
                return;
            }
            IsAddressEnabled = false;
        }

        private bool CanAddCubeToExistingEntry(object obj)
        {
            return true;
        }

        private void AddCubeToExistingEntry(object obj)
        {
            throw new NotImplementedException();
        }

        private bool CanSaveRepository(object obj)
        {
            return true;
        }

        private void SaveRepository(object obj)
        {
            if (_repositoryProxy.IsRepositoryExist)
            {
                _repositoryProxy.SaveRepository(new FileInfo(_repositoryFileName));
            }
            
        }

        private bool CanCreateEmptyRepository(object obj)
        {
            return true;
        }

        private void CreateEmptyRepository(object obj)
        {
            var repFile = new FileInfo(_repositoryFileName);
            _repositoryProxy.CreateEmptyRepository();
            if (true == _repositoryProxy.SaveRepository(repFile))
            {
                PanelLogMessage = $"Backing file {repFile.FullName} loaded.";
                //RepositoryFullName = repFile.FullName;
            }
        }

        private bool CanAddCubeToNewEntry(object obj)
        {
            return true;
        }

        private void AddCubeToNewEntry(object obj)
        {
            if (_repositoryProxy.IsRepositoryExist)
            {
                List<CubeType> cubeTypes = _repositoryProxy.GetCubeTypesBySignature(CubeSignature);

                // select to which cube-type to add current cube


                // add new cube-type
                if ((null == CubeNickname) || (null == CubeDesc))
                {
                    MessageBox.Show("Must fill both name and desc");
                }
                else
                {
                    CubeType ct = _repositoryProxy.AddCubeType(CubeNickname, CubeDesc, CubeSignature);
                    ct.PertainCubeList.Add( CubeAddress.ToString());
                }

            }

        }

        private bool CanGetCubeSignature(object obj)
        {
            return true;
        }

        private void GetCubeSignature(object obj)
        {
            if (null == _cubeAddress)
            {
                return;
            }
            List<UeiDeviceInfo> devList = null;
            if (_cubeAddress.Equals(IPAddress.Any))
            {
                devList = CubeSeeker.GetDeviceList("simu://");
            }
            else if (null != CubeSeeker.TryIP(_cubeAddress))
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
            return true;
        }

        private void GetFreeIp(object obj)
        {

            if (_repositoryProxy.IsRepositoryExist)
            {

                List<IPAddress> ipList = _repositoryProxy.GetAllPertainedCubes();
                List<byte> lsbList = ipList.Select(i => i.GetAddressBytes()[3]).ToList();
                //lsbList.Sort();
                int max = (lsbList.Count > 0) ? lsbList.Max() : 1;
                ++max;
                //byte last = lsbList[lsbList.Count - 1];
                IPAddress ipa = new IPAddress(new byte[] { 192, 168, 100, (byte)max });
                IsAddressEnabled = true;
                CubeAddress = ipa;
            }
        }
    }
}
