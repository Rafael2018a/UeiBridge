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

namespace UeiBridge.CubeNet
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MainViewModel _vm;
        public MainWindow()
        {
            InitializeComponent();
            _vm = new MainViewModel( this);
            DataContext = _vm;

            _vm.MessageBoxEvent += _vm_MessageBoxEvent;
        }

        private void _vm_MessageBoxEvent(string message)
        {
            MessageBox.Show(message, "CubeNet", MessageBoxButton.OK);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _vm.ExitAppCommand.Execute(e);
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source is TabControl)
            {
                if (GenerateSetupFile.IsSelected)
                {
                    TabControl tc = e.Source as TabControl;
                    _vm.GetRepositoryEntriesCommand.Execute(e);
                }
            }
        }

        private void GenerateSetup_click(object sender, RoutedEventArgs e)
        {
            //var i = CubeTypeList.SelectedIndex;
            //var z = CubeTypeList.SelectedItem;
            //CubeType ct = z as CubeType;
            _vm.GenerateSetupFileCommand.Execute(CubeTypeList.SelectedItem);
        }

        private void MatchingCubeTypeList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _vm.OnMatchingCubeTypeListChange();
        }
    }
}
