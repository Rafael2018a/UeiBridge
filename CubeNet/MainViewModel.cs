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
        public RelayCommand GetFreeIpCommand { get; set; }
        public RelayCommand GetCubeSignatureCommand { get; set; }
        public RelayCommand UpdateLocalRepCommand { get; set; }

        string _cubeSignature;
        IPAddress _cubeAddress;
        string _messageToUser;
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

        CubeRepositoryProxy _repProxy = new CubeRepositoryProxy();
        //IPAddress _cubeIp;
        public MainViewModel()
        {
            LoadCommands();
        }
        private void LoadCommands()
        {
            GetFreeIpCommand = new RelayCommand(GetFreeIp, CanGetFreeIp);
            GetCubeSignatureCommand = new RelayCommand(GetCubeSignature, CanGetCubeSignature);
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

            if (null!=CubeSeeker.TryIP(_cubeAddress))
            {
                List<string> devList = CubeSeeker.GetDeviceNames(_cubeAddress);
            }

            //bool cubeExist = CubePing(_cubeIp);
            CubeSignature = "a-b-c";
        }

        private bool CanGetFreeIp(object obj)
        {
            return true;
        }

        private void GetFreeIp(object obj)
        {
            FileInfo repFile = null;
            CubeRepository rep = _repProxy.LoadRepository(repFile);
            List<IPAddress> ipList = _repProxy.GetAllPertainedCubes();
            //List<IPAddress> ipList = ipStrList.Select(i => IPAddress.Parse(i)).ToList();
            List<byte> lsbList = ipList.Select(i => i.GetAddressBytes()[3]).ToList();
            //lsbList.Sort();
            byte max = lsbList.Max();
            max++;
            byte last = lsbList[lsbList.Count - 1];
            IPAddress ipa = new IPAddress( new byte[] { 192, 168, 100, max });

            CubeAddress = ipa;
        }
    }
}
