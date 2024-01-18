using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using UeiBridge.Library.CubeSetupTypes;
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

        public CubeSetup CubeSetupMain { get; set; }
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

            var EntAsm = UeiBridge.Library.StaticMethods.GetLibVersion();
            System.IO.FileInfo fi = new System.IO.FileInfo(EntAsm.Location);

            MainWindowTitle = $"Cube Design. {EntAsm.GetName().Version.ToString(4)}. (Build time: {fi.LastWriteTime.ToString()})";

            _parentView = parentView;
        }

        private bool CanCloseFile(object obj)
        {
            return true;
        }

        private void CloseFile(object obj)
        {
            bool isClean = true;

            if (CubeSetupMain!=null)
            {
                isClean = CubeSetupMain.Equals(CubeSetupClean);
            }

            if (isClean)
            {
                CubeSetupMain = null;
                systemSetupVM = null;
                return;
            }
            MessageBoxResult mbr = MessageBox.Show("Close without saving changes?", "User", MessageBoxButton.YesNo);
            if (mbr == MessageBoxResult.Yes)
            {
                CubeSetupMain = null;
                systemSetupVM = null;
                return;
            }
        }

        private bool CanSaveFile(object arg)
        {
            if (null == CubeSetupMain)
            {
                return false;
            }
            return !CubeSetupMain.Equals(CubeSetupClean);
        }

        FileInfo _loadedSetupFile;
        public void LoadSetupFile(FileInfo configFile)
        {
            if (!configFile.Exists)
            {
                MessageBox.Show($"File {configFile.FullName} not exists", "Error", MessageBoxButton.OK);
                return;
            }
            try
            {
                CubeSetupLoader cslMain = new CubeSetupLoader(configFile); 
                CubeSetupMain = cslMain.CubeSetupMain;
                CubeSetupLoader cslClean = new CubeSetupLoader(configFile);
                CubeSetupClean = cslClean.CubeSetupMain;

                System.Diagnostics.Debug.Assert(null != CubeSetupMain); 
                System.Diagnostics.Debug.Assert(null != CubeSetupClean);

                MidStatusBarMessage = $"Setup file: {configFile.Name}";
                _loadedSetupFile = configFile;

                var sysVM = new SystemSetupViewModel(new List<CubeSetup>() { CubeSetupMain });
                systemSetupVM = sysVM;

            }
            catch (System.IO.FileNotFoundException ex)
            {
                CubeSetupMain = new CubeSetup();
                MidStatusBarMessage = $"{ex.Message}";
            }
            catch (System.InvalidOperationException ex) // bad formatted xml
            {
                CubeSetupMain = new CubeSetup();
                MidStatusBarMessage = $"Setup file ({Config2.DefaultSettingsFilename}) parse error. {ex.Message}";
                MessageBox.Show(MidStatusBarMessage, "Error", MessageBoxButton.OK);
            }

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
            CubeSetupLoader.SaveSetupFile( CubeSetupMain, new FileInfo( _loadedSetupFile.Name));//  As(new FileInfo(Config2.DefaultSettingsFilename), true);
            System.Threading.Thread.Sleep(10);
            CubeSetupLoader csl = new CubeSetupLoader(_loadedSetupFile);
            
            CubeSetupClean = csl.CubeSetupMain;
            System.Diagnostics.Debug.Assert(null != CubeSetupClean);
                //CubeSetup.LoadCubeSetupFromFile( new FileInfo( CubeSetup1.AssociatedFileFullname));
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
            if (CubeSetupMain != null)
            {
                isClean = CubeSetupMain.Equals(CubeSetupClean);
            }

            if (isClean)
            {
                return;
            }
            MessageBoxResult mbr = MessageBox.Show("Save changes?", "User", MessageBoxButton.YesNoCancel);
            if (mbr == MessageBoxResult.Cancel)
            {
                CancelEventArgs e = param as CancelEventArgs;
                e.Cancel = true;
            }
            if (mbr == MessageBoxResult.Yes)
            {
                SaveFile(null);
            }
        }
    }
}
