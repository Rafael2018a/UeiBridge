using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UeiBridge.Library;

namespace UeiBridgeSetup.ViewModels
{
    class MainViewModel : ViewModelBase
    {
        private string _midStatusBarMessage;
        //private string _settingFileName;

        public event Action<SystemSetupViewModel> newSystemViewModel;
        public DelegateCommand OpenFileCommand { get; }

        public Config2 MainConfig { get; set; }

        public string MidStatusBarMessage 
        { 
            get => _midStatusBarMessage;
            set 
            { 
                _midStatusBarMessage = value;
                RaisePropertyChanged();
            }
        }
        public MainViewModel()
        {
            OpenFileCommand = new DelegateCommand(OpenFile);
            //_settingFileName = Config2.DafaultSettingsFilename;

            LoadConfig( new FileInfo(Config2.DafaultSettingsFilename));
            
        }

        public void LoadConfig( FileInfo configFile)
        {
            try
            {
                MainConfig = Config2.LoadConfigFromFile(configFile);
                MidStatusBarMessage = $"Setup file: {Config2.DafaultSettingsFilename}";
            }
            catch(System.IO.FileNotFoundException ex)
            {
                MainConfig = new Config2();
                MidStatusBarMessage = $"{ex.Message}";
            }
            catch(System.InvalidOperationException ex)
            {
                MainConfig = new Config2();
                MidStatusBarMessage = $"Setup file ({Config2.DafaultSettingsFilename}) parse error. {ex.Message}";
            }

            var sysVM = new SystemSetupViewModel(MainConfig);
            newSystemViewModel?.Invoke(sysVM);
        }

        private void OpenFile(object parameter)
        {
            string a = parameter as string;

            var dialog2 = new Microsoft.Win32.OpenFileDialog();

            //dialog2.FileName = "Document"; // Default file name
            dialog2.DefaultExt = ".Config"; // Default file extension
            dialog2.Filter = "Config file (.config)|*.config"; // Filter files by extension

            // Show open file dialog box
            bool? result = dialog2.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                // Open document
                string filename = dialog2.FileName;
            }

            LoadAsync(parameter);
        }

        private void LoadAsync(object parameter)
        {

        }
    }
}
