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

namespace ByteStreamer3
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            {
                //List<User> items = new List<User>();
                //items.Add(new User() { Name = "John Doe", Age = 42 , Mail="h1@g.mail"});
                //items.Add(new User() { Name = "Jane Doe", Age = 39 });
                //items.Add(new User() { Name = "Sammy Doe", Age = 13 });
                //lvDataBinding.ItemsSource = items;
            }
            DataContext = new MainViewModel();
        }
    }
    public class PlayItem
    {
        public string Name { get; set; }

        public int PlayedBlocks { get; set; }
        public string Mail { get; set; }
        //public override string ToString()
        //{
        //    return this.Name + ", " + this.Age + " years old";
        //}
    }
}
