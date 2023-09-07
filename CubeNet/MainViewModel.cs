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

namespace UeiBridge.CubeNet
{
    class MainViewModel : ViewModelBase
    {
        #region == commands ==
        public RelayCommand CreateEmptyRepositoryCommand { get; set; }
        public RelayCommand GetFreeIpCommand { get; set; }
        public RelayCommand GetCubeSignatureCommand { get; set; }
        public RelayCommand AddCubeToRepositoryCommand { get; set; }
        public RelayCommand SaveRepositoryCommand { get; set; }
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
        string _repositoryState;
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

        public string RepositoryState { get => _repositoryState; set { _repositoryState = value; RaisePropertyChanged(); } }

        public string CubeNickname { get; set; }
        public string CubeDesc { get; set; }

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
                    RepositoryState = $"Backing file {_repositoryProxy.RepositoryBackingFile.FullName} loaded";
                }
                catch (Exception ex)
                {
                    RepositoryState = $"Failed to load file {repFile.FullName}. {ex.Message}";
                }
            }
            else
            {
                RepositoryState = $"Backing file {repFile.FullName} doesn't exist.";
            }

        }
        private void LoadCommands()
        {
            CreateEmptyRepositoryCommand = new RelayCommand(CreateEmptyRepository, CanCreateEmptyRepository);
            GetFreeIpCommand = new RelayCommand(GetFreeIp, CanGetFreeIp);
            GetCubeSignatureCommand = new RelayCommand(GetCubeSignature, CanGetCubeSignature);
            AddCubeToRepositoryCommand = new RelayCommand(AddCubeToRepository, CanAddCubeToRepository);
            SaveRepositoryCommand = new RelayCommand( SaveRepository, CanSaveRepository);
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
                RepositoryState = $"Backing file {repFile.FullName} loaded.";
                //RepositoryFullName = repFile.FullName;
            }
        }

        private bool CanAddCubeToRepository(object obj)
        {
            return true;
        }

        private void AddCubeToRepository(object obj)
        {
            if (_repositoryProxy.IsRepositoryExist)
            {
                List<CubeType> cubeTypes = _repositoryProxy.GetCubeTypesBySignature(CubeSignature);

                // select to which cube-type to add current cube


                // add new cube-type
                _repositoryProxy.AddCubeType( CubeNickname, CubeDesc, CubeSignature);

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

                CubeAddress = ipa;
            }
        }
    }
}
