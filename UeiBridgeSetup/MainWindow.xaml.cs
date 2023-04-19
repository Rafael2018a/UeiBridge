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
using UeiBridgeSetup.ViewModels;

namespace UeiBridgeSetup
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MainViewModel mvm;
        public MainWindow()
        {
            InitializeComponent();
            mvm = new MainViewModel();
            this.DataContext = mvm;
            mvm.newSystemViewModel += SetSystemViewModel;

            mvm.LoadConfig(new System.IO.FileInfo(Config2.DafaultSettingsFilename));

            

            
        }

        private void SetSystemViewModel(SystemSetupViewModel sysVM)
        {
            _systemSetupView.DataContext = sysVM;
        }
    }
}
