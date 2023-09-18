using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UeiBridge.Library;

namespace CubeDesign.ViewModels
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

        public CubeSetup CubeSetup1 { get; set; }

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
                CubeSetup1 = Config2.LoadCubeSetupFromFile( configFile.Name);
                MidStatusBarMessage = $"Setup file: {Config2.DefaultSettingsFilename}";
            }
            catch (System.IO.FileNotFoundException ex)
            {
                CubeSetup1 = new CubeSetup();
                MidStatusBarMessage = $"{ex.Message}";
            }
            catch (System.InvalidOperationException ex)
            {
                CubeSetup1 = new CubeSetup();
                MidStatusBarMessage = $"Setup file ({Config2.DefaultSettingsFilename}) parse error. {ex.Message}";
            }

            _menuItemHeader_Save = $"Save {configFile.Name}";
            _menuItemHeader_SaveAs = _menuItemHeader_Save + " As";

            var sysVM = new SystemSetupViewModel( new List<CubeSetup>() { CubeSetup1 });
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

        private void SaveFile(object param) 
        {
            CubeSetup1.Serialize();//  As(new FileInfo(Config2.DefaultSettingsFilename), true);
        }
        private void SaveFileAs(object param) { }
        private void CloseApp(object param) { }
    }
}
