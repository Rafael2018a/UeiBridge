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
using UeiBridgeSetup.ViewModels;

namespace UeiBridgeSetup
{
    /// <summary>
    /// Interaction logic for DeviceSetupView.xaml
    /// </summary>
    public partial class DeviceSetupView : UserControl
    {
        private DeviceSetupViewModel _viewModel;

        public DeviceSetupView()
        {
            InitializeComponent();
            _viewModel = new DeviceSetupViewModel();
            DataContext = _viewModel;
        }
    }
}
