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

namespace CubeDesign.Views
{
    /// <summary>
    /// Interaction logic for SL508View.xaml
    /// </summary>
    public partial class SL508View : UserControl
    {
        public SL508View()
        {
            InitializeComponent();

            ModeCombo.ItemsSource = Enum.GetValues(typeof(UeiDaq.SerialPortMode));//.Cast<UeiDaq.SerialPortMode>();
            BaudCombo.ItemsSource = Enum.GetValues(typeof(UeiDaq.SerialPortSpeed));
            ParityCombo.ItemsSource = Enum.GetValues(typeof(UeiDaq.SerialPortParity));
            StopbitsCombo.ItemsSource = Enum.GetValues(typeof(UeiDaq.SerialPortStopBits));

        }
}
}
