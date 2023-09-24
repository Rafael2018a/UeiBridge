﻿using System;
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
            _vm = new MainViewModel();
            DataContext = _vm;
        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _vm.ExitAppCommand.Execute(e);
        }

        //private void Button_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Please use PowerDNA explorer to change the physical cube address\nAfter that, click \"Get cube signature\"", "Cube IP", MessageBoxButton.OK);
        //    SuggestedIp.IsEnabled = false;
        //}
    }
}