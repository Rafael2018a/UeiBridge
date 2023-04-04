using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UeiBridge.Library;

namespace UeiBridgeSetup.ViewModels
{
    class MainViewModel: ViewModelBase
    {
        //const string _defaultSetupFilename = "UeiSettings2.config";

        public DelegateCommand AddCommand { get; }

        //UeiBridge.Library.Config2 c2;

        public MainViewModel()
        {
            AddCommand = new DelegateCommand(Add);
            var bs = Config2.Instance.Blocksensor;
        }
        void LoadSetupFile( string fileFullpath)
        {
            if (!File.Exists(Config2.DafaultSettingsFilename))
            {
                return;
            }
        }

        private void Add(object parameter)
        {
            string a = parameter as string;
            LoadAsync( parameter);
        }

        private void LoadAsync(object parameter)
        {
            
        }
    }
}
