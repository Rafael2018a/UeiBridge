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
        private string _menuItemHeader_Save;
        private string _menuItemHeader_SaveAs;

        //private string _settingFileName;

        public event Action<SystemSetupViewModel> OnNewSystemViewModel;
        public DelegateCommand OpenFileCommand { get; }
        public DelegateCommand SaveFileCommand { get; }
        public DelegateCommand SaveFileAsCommand { get; }
        public DelegateCommand CloseAppCommand { get; }

        public Config2 MainConfig { get; set; }

        public string MenuItemHeader_Save { get => _menuItemHeader_Save; set => _menuItemHeader_Save = value; }
        public string MenuItemHeader_SaveAs { get => _menuItemHeader_SaveAs; set => _menuItemHeader_SaveAs = value; }

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
            SaveFileCommand = new DelegateCommand(SaveFile);
            SaveFileAsCommand = new DelegateCommand(SaveFileAs);
            CloseAppCommand = new DelegateCommand(CloseApp);

        }

        public void LoadSetupFile(FileInfo configFile)
        {
            try
            {
                MainConfig = Config2.LoadConfigFromFile(configFile);
                MidStatusBarMessage = $"Setup file: {Config2.DafaultSettingsFilename}";
            }
            catch (System.IO.FileNotFoundException ex)
            {
                MainConfig = new Config2();
                MidStatusBarMessage = $"{ex.Message}";
            }
            catch (System.InvalidOperationException ex)
            {
                MainConfig = new Config2();
                MidStatusBarMessage = $"Setup file ({Config2.DafaultSettingsFilename}) parse error. {ex.Message}";
            }

            _menuItemHeader_Save = $"Save {configFile.Name}";
            _menuItemHeader_SaveAs = _menuItemHeader_Save + " As";

            var sysVM = new SystemSetupViewModel(MainConfig);
            OnNewSystemViewModel?.Invoke(sysVM);
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
                LoadSetupFile(new FileInfo(filename));
            }

        }

        private void SaveFile(object param) { }
        private void SaveFileAs(object param) { }
        private void CloseApp(object param) { }
    }
}
