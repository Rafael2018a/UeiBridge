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

        

        public MainViewModel()
        {
            AddCommand = new DelegateCommand(Add);
            //var bs = Config2.Instance.Blocksensor;

            var x = UeiBridge.Library.Config2.DafaultSettingsFilename;
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
