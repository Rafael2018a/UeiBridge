using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using UeiBridge.Library;
using CubeDesign.ViewModels;
using System.ComponentModel;

namespace CubeDesign
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MainViewModel _mainVM;
        public MainWindow()
        {
            InitializeComponent();
            _mainVM = new MainViewModel();
            this.DataContext = _mainVM;
            _mainVM.OnNewSystemViewModel += SetSystemViewModel;

            _mainVM.LoadSetupFile(new System.IO.FileInfo("cube3.config")); //Config2.DefaultSettingsFilename););
            
        }
        private void SetSystemViewModel(SystemSetupViewModel sysVM)
        {
            _systemSetupView.DataContext = sysVM;
        }

        //protected override void OnClosing(CancelEventArgs e)
        //{
        //    base.OnClosing(e);
        //}
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            
            //if (_mainVM.IsConfigDirty)
            //{
            //    _mainVM.AskToSaveFile
            //    e.Cancel = true;
            //}
        }
    }
}
