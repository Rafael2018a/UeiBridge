using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Globalization;
using UeiBridge.Library;

namespace UeiBridge.CubeNet
{
    class MainViewModel : ViewModelBase
    {
        #region == commands ==
        public RelayCommand CreateEmptyRepositoryCommand { get; set; }
        public RelayCommand GetFreeIpCommand { get; set; }
        public RelayCommand GetCubeSignatureCommand { get; set; }
        public RelayCommand AddCubeToRepositoryCommand { get; set; }
        #endregion
        #region == privates ==
        string _cubeSignature;
        IPAddress _cubeAddress;
        string _messageToUser;
        //FileInfo _repositoryFile;
        const string _repositoryFileName = "CubeRepository.json";
        CubeRepositoryProxy _repositoryProxy = new CubeRepositoryProxy();
        string _repositoryFullName;
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
        public string RepositoryFullName
        {
            get => _repositoryFullName;
            set { _repositoryFullName = value; RaisePropertyChanged(); }
        }
        #endregion

        //IPAddress _cubeIp;
        public MainViewModel()
        {
            LoadCommands();

            var repFile = new FileInfo(_repositoryFileName);
            RepositoryFullName = "<no repository loaded>";
            if (repFile.Exists)
            {
                if (true == _repositoryProxy.LoadRepository(repFile))
                {
                    RepositoryFullName = repFile.FullName;
                }
            }
        }
        private void LoadCommands()
        {
            CreateEmptyRepositoryCommand = new RelayCommand(CreateEmptyRepository, CanCreateEmptyRepository);
            GetFreeIpCommand = new RelayCommand(GetFreeIp, CanGetFreeIp);
            GetCubeSignatureCommand = new RelayCommand(GetCubeSignature, CanGetCubeSignature);
            AddCubeToRepositoryCommand = new RelayCommand(AddCubeToRepository, CanAddCubeToRepository);
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
                RepositoryFullName = repFile.FullName;
            }
        }

        private bool CanAddCubeToRepository(object obj)
        {
            return true;
        }

        private void AddCubeToRepository(object obj)
        {
            throw new NotImplementedException();
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

            if (null != CubeSeeker.TryIP(_cubeAddress))
            {
                List<UeiDeviceInfo> devList = CubeSeeker.GetDeviceList(_cubeAddress);
                StringBuilder sb = new StringBuilder();
                foreach (UeiDeviceInfo udi in devList)
                {
                    sb.Append(udi.DeviceName);
                    sb.Append("/");
                }
                //sb.Remove(sb.Length - 1, 1);
                CubeSignature = sb.ToString();
            }
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
                int max = (lsbList.Count>0) ? lsbList.Max() : 1;
                ++max;
                //byte last = lsbList[lsbList.Count - 1];
                IPAddress ipa = new IPAddress(new byte[] { 192, 168, 100, (byte)max });

                CubeAddress = ipa;
            }
        }
    }
}
