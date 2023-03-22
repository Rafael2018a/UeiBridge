using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace BridgeSetup
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MenuItem root = new MenuItem() { Title = "Menu" };
            MenuItem childItem1 = new MenuItem() { Title = "Child item #1", ImageSource = "/Images/Region.png" };
            childItem1.Items.Add(new MenuItem() { Title = "Child item #1.1", ImageSource = "/Images/icons8-computer-64.png" });
            childItem1.Items.Add(new MenuItem() { Title = "Child item #1.2", ImageSource = "/City.png" });
            root.Items.Add(childItem1);
            root.Items.Add(new MenuItem() { Title = "Child item #2" });
            trvMenu.Items.Add(root);

        }
    }

    public class MenuItem
    {
        public MenuItem()
        {
            this.Items = new ObservableCollection<MenuItem>();
        }

        public string Title { get; set; }
        public string ImageSource { get; set; } // => "/Images/Region.png";
        public ObservableCollection<MenuItem> Items { get; set; }
    }

}
