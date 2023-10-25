using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using UeiBridge.Library;

namespace CubeDesign.ViewModels
{
    class MainViewModel : ViewModelBase
    {
        private string _midStatusBarMessage;
        private string _menuItemHeader_Save;
        private string _menuItemHeader_SaveAs;

        public string MainWindowTitle { get; }
        //private string _settingFileName;

        Window _parentView;

        //public event Action<SystemSetupViewModel> OnNewSystemViewModel;
        public RelayCommand OpenFileCommand { get; }
        public RelayCommand SaveFileCommand { get; }
        //public RelayCommand SaveFileAsCommand { get; }
        public RelayCommand ExitAppCommand { get; }
        public RelayCommand CloseFileCommand { get; }

        public CubeSetup CubeSetup1 { get; set; }
        public CubeSetup CubeSetupClean { get; set; }

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
        public MainViewModel(Window parentView)
        {
            OpenFileCommand = new RelayCommand(OpenFile);
            SaveFileCommand = new RelayCommand(SaveFile, CanSaveFile); 
            //SaveFileAsCommand = new RelayCommand(SaveFileAs);
            ExitAppCommand = new RelayCommand(ExitApp);
            CloseFileCommand = new RelayCommand(CloseFile, CanCloseFile);

            MainWindowTitle = "Cube Design";

            _parentView = parentView;
        }

        private bool CanCloseFile(object obj)
        {
            return true;
        }

        private void CloseFile(object obj)
        {
            bool isClean = true;

            if (CubeSetup1!=null)
            {
                isClean = CubeSetup1.Equals(CubeSetupClean);
            }

            if (isClean)
            {
                CubeSetup1 = null;
                systemSetupVM = null;
                return;
            }
            MessageBoxResult mbr = MessageBox.Show("Close without saving changes?", "User", MessageBoxButton.YesNo);
            if (mbr == MessageBoxResult.Yes)
            {
                CubeSetup1 = null;
                systemSetupVM = null;
                return;
            }
        }

        private bool CanSaveFile(object arg)
        {
            if (null == CubeSetup1)
            {
                return false;
            }
            return !CubeSetup1.Equals(CubeSetupClean);
        }

        public void LoadSetupFile(FileInfo configFile)
        {
            if (!configFile.Exists)
            {
                MessageBox.Show($"File {configFile.FullName} not exists", "Error", MessageBoxButton.OK);
                return;
            }
            try
            {
                CubeSetup1 = CubeSetup.LoadCubeSetupFromFile( configFile);
                CubeSetupClean = CubeSetup.LoadCubeSetupFromFile( configFile);
                MidStatusBarMessage = $"Setup file: {configFile.Name}";
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
                MessageBox.Show(MidStatusBarMessage, "Error", MessageBoxButton.OK);
            }

            //_menuItemHeader_Save = $"Save {configFile.Name}";
            //_menuItemHeader_SaveAs = _menuItemHeader_Save + " As";

            var sysVM = new SystemSetupViewModel( new List<CubeSetup>() { CubeSetup1 });
            systemSetupVM = sysVM;
            //OnNewSystemViewModel?.Invoke(sysVM);
        }
        SystemSetupViewModel _systemSetupVM;
        public SystemSetupViewModel systemSetupVM 
        { 
            get => _systemSetupVM;
            set 
            { 
                _systemSetupVM = value;
                RaisePropertyChanged();
            }
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
            CubeSetupClean = CubeSetup.LoadCubeSetupFromFile( new FileInfo( CubeSetup1.AssociatedFileFullname));
        }
        private void SaveFileAs(object param) { }
        private void ExitApp(object param) 
        {
            if (null == param) // if menu item 'close' clicked
            {
                _parentView.Close(); // this call will finally call this method again
                return;
            }

            bool isClean = true;
            if (CubeSetup1 != null)
            {
                isClean = CubeSetup1.Equals(CubeSetupClean);
            }

            if (isClean)
            {
                return;
            }
            MessageBoxResult mbr = MessageBox.Show("Exit without saving changes?", "User", MessageBoxButton.YesNo);
            if (mbr == MessageBoxResult.No)
            {
                CancelEventArgs e = param as CancelEventArgs;
                e.Cancel = true;
            }
        }
    }
}
