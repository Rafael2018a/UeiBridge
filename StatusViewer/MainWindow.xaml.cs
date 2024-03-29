﻿using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Configuration;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UeiBridge.Library;
using System.Text;

namespace StatusViewer
{
    //public class Rootobject
    //{
    //    public DateTime date { get; set; }
    //    public string level { get; set; }
    //    public string logger { get; set; }
    //    public string message { get; set; }
    //}

    public enum MachineStateEnum { Initial, Running, Freeze }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, System.ComponentModel.INotifyPropertyChanged
    {
        System.Net.Sockets.UdpClient m_udpListener;

        public ObservableCollection<StatusEntryViewModel> EntriesList { get; set; } = new ObservableCollection<StatusEntryViewModel>();

        MachineStateEnum _machineState = MachineStateEnum.Initial;
        long _receivedBytes = 0;
        long _receivedDatagrams = 0;

        //Process dbgViewProcess;

        readonly List<string> logDic = new List<string>();

        IPAddress m_multicastIp;
        public IPAddress MulticastIp
        {
            get { return m_multicastIp; }
            set
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("MulticastIp"));
                m_multicastIp = value;
            }
        }
        int mcPort;
        public string MulticastEP
        {
            get => new IPEndPoint(m_multicastIp, mcPort).ToString();
        }

        //List<IPAddress>  m_localIpList;
        public List<IPAddress> LocalIpList { get; set; }
        //{get { return m_localIpList; }}
        public IPAddress SelectedLocalIp { get; set; }
        StatusEntryViewModel TryGetValue(ObservableCollection<StatusEntryViewModel> oc, string desc)
        {
            //StatCounter existingStatCounter = null;
            foreach (StatusEntryViewModel sc in EntriesList)
            {
                if (sc.Desc == desc)
                {
                    return sc;
                }
            }

            return null;
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            logDic.Add("EMERG");
            logDic.Add("ALERT");
            logDic.Add("CRIT");
            logDic.Add("ERROR");
            logDic.Add("WARNING");
            logDic.Add("NOTICE");
            logDic.Add("INFO");
            logDic.Add("DEBUG");


            //StatusEntryJson jsonStatus = new StatusEntryJson("test desc", "test val", StatusTrait.IsRegular);
            //m_entriesList.Add(new StatusEntryViewModel(new StatusEntryModel(jsonStatus)));
            //jsonStatus = new StatusEntryJson("waring desc", "warn val", StatusTrait.IsWarning);
            //m_entriesList.Add(new StatusEntryViewModel(new StatusEntryModel(jsonStatus)));

            SetCommands();

            LocalIpList = GetLocalIpList();

            //m_entriesList.Clear();

            //ProcessStartInfo psi = new ProcessStartInfo(@".\Dbgview.exe", "/f");
            //dbgViewProcess = Process.Start(psi);

            StartCommand_Executed(this, null); // simulate 'start' command (as if user clicked 'start' right after startup)
#if testonly
            Task.Factory.StartNew(() =>
            {
                for (int i = 0; i < 10; i++)
                {
                    UdpClient udpClient = new UdpClient();
                    System.Threading.Thread.Sleep(200);

                    string[] slist = new string[2];
                    slist[0]=  $"Regular message {i}";
                    slist[1] = $"line 2 {i * 2}";
                    StatusEntryJson js = new StatusEntryJson("Test message", slist , StatusTrait.IsRegular);
                    StatusEntryJson js1 = new StatusEntryJson( "Warn message", new string[] { $"Warning message {i}" }, StatusTrait.IsWarning);
                    string s = Newtonsoft.Json.JsonConvert.SerializeObject(js);
                    byte[] send_buffer = Encoding.ASCII.GetBytes(s);
                    string s1 = Newtonsoft.Json.JsonConvert.SerializeObject(js1);
                    byte[] send_buffer1 = Encoding.ASCII.GetBytes(s1);

                    try
                    {
                        string ip1 = ConfigurationManager.AppSettings["multicastIp"];
                        int port = Int32.Parse( ConfigurationManager.AppSettings["multicastPort"]);

                        IPAddress ip = IPAddress.Parse(ip1);
                        udpClient.Send(send_buffer, send_buffer.Length, new IPEndPoint(ip, port));
                        udpClient.Send(send_buffer1, send_buffer1.Length, new IPEndPoint(ip, port));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                }
            });
#endif
        }
        private List<IPAddress> GetLocalIpList()
        {
            List<IPAddress> ipList = new List<IPAddress>();
            string hostName = Dns.GetHostName();
            IPHostEntry ipEntry = Dns.GetHostEntry(hostName);
            //SelectedLocalIp = ipEntry.AddressList[0];
            foreach (IPAddress ipa in ipEntry.AddressList)
            {
                if (ipa.AddressFamily == AddressFamily.InterNetwork)
                {
                    ipList.Add(ipa);
                    //SelectedLocalIp = ipa;
                }
            }
            //ipList.Add(IPAddress.Any);
            SelectedLocalIp = ipList[ipList.Count - 1];
            return ipList;
        }
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            //if (!dbgViewProcess.HasExited)
            //    dbgViewProcess.Kill();
            //dbgViewProcess.Dispose();

        }

        private void SetCommands()
        {
            CommandBinding startCommand = new CommandBinding(MediaCommands.Play);
            this.CommandBindings.Add(startCommand);
            startCommand.Executed += new ExecutedRoutedEventHandler(StartCommand_Executed);
            startCommand.CanExecute += StartCommand_CanExecute;

            CommandBinding stopCommand = new CommandBinding(MediaCommands.Stop);
            this.CommandBindings.Add(stopCommand);
            stopCommand.Executed += StopCommand_Executed;
            stopCommand.CanExecute += StopCommand_CanExecute;

            CommandBinding freezeCommand = new CommandBinding(MediaCommands.Pause);
            this.CommandBindings.Add(freezeCommand);
            freezeCommand.Executed += new ExecutedRoutedEventHandler(FreezeCommandExecuted);
            freezeCommand.CanExecute += FreezeCommand_CanExecute;

            CommandBinding clearAllCommand = new CommandBinding(MediaCommands.ChannelDown);  // Hmmm ChannelDown is the best choise
            this.CommandBindings.Add(clearAllCommand);
            clearAllCommand.Executed += ClearAllCommand_Executed;
            clearAllCommand.CanExecute += ClearAllCommand_CanExecute;
        }

        /// <summary>
        /// This callback should be called upon receiving datagram from publisher
        /// </summary>
        void UdpReceiveCallback(IAsyncResult asyncResult)
        {
            Tuple<UdpClient, IPEndPoint> udpState = (Tuple<UdpClient, IPEndPoint>)asyncResult.AsyncState;
            UdpClient udpListener = udpState.Item1;
            IPEndPoint ep = udpState.Item2;
            byte[] receiveBuffer = null;
            StatusEntryJson js;
            try // just in case socket was closed before reaching here
            {
                receiveBuffer = udpListener.EndReceive(asyncResult, ref ep);
                string str = Encoding.Default.GetString(receiveBuffer);
                js = JsonConvert.DeserializeObject<StatusEntryJson>(str);
            }
            catch (ObjectDisposedException) // socket already closed
            {
                return;
            }

            ReceivedBytes += receiveBuffer.Length;
            ReceivedDatagrams++;

            StatusEntryModel messageModel = new StatusEntryModel(js);

            // create or update counter entry
            // ==============================
            StatusEntryViewModel baseViewModel = TryGetValue(EntriesList, messageModel.Desc);
            if (baseViewModel != null) // if object already exist
            {
                if (MachineState != MachineStateEnum.Freeze)
                {
                    StatusEntryViewModel vm = baseViewModel as StatusEntryViewModel;
                    vm.Update(messageModel);
                }
            }
            else // object not exists. create new one
            {
                StatusEntryViewModel vm = new StatusEntryViewModel(messageModel);
                Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.DataBind, new Action(() => EntriesList.Add(vm)));
            }

            udpListener.BeginReceive(new AsyncCallback(UdpReceiveCallback), udpState);
        }


        private void StopCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ((MachineState == MachineStateEnum.Running) || (MachineState == MachineStateEnum.Freeze)); // 
        }

        private void StopCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MachineState = MachineStateEnum.Initial;
            togglebuttonFreezeDispaly.IsChecked = false;

            //uClient.DropMulticastGroup(;//_mcAddress);
            m_udpListener.Close();


        }

        private void ClearAllCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void ClearAllCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            EntriesList.Clear();
            //ReceivedDatagrams = 0;
        }


        private void StartCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (MachineState == MachineStateEnum.Initial);
        }

        private void StartCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            StartMulticast();
            MachineState = MachineStateEnum.Running;
            //StatusCounterViewModel.EnableBindingUpdate = true; // just in case we came here after freeze


        }

        private void StartMulticast()
        {
            IPAddress mcAddress = null;
            string mcIp = ConfigurationManager.AppSettings["multicastIp"];
            mcAddress = (mcIp != null) ? IPAddress.Parse(mcIp) : IPAddress.Parse("239.10.10.17"); // get from config or use default
            string mcPort1 = ConfigurationManager.AppSettings["multicastPort"];
            mcPort = (mcPort1 != null) ? Int32.Parse(mcPort1) : 5093; // get from config or use default
            AppServices.WriteToTrace(string.Format("Multicast EP: {0}:{1}", mcAddress, mcPort));

            // define listener
            try
            {
                m_udpListener = new System.Net.Sockets.UdpClient();
                IPEndPoint localEp = new IPEndPoint(SelectedLocalIp, mcPort); // this is just for the port number
                //IPEndPoint localEp = new IPEndPoint( IPAddress.Any, mcPort); // this is just for the port number
                m_udpListener.Client.Bind(localEp);

                m_udpListener.JoinMulticastGroup(mcAddress, SelectedLocalIp);//IPAddress.Parse("192.168.1.128")); // ip of UAV-LAN
                m_multicastIp = mcAddress;
                Tuple<UdpClient, IPEndPoint> udpState = new Tuple<UdpClient, IPEndPoint>(m_udpListener, localEp);
                m_udpListener.BeginReceive(new AsyncCallback(UdpReceiveCallback), udpState);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create socket. " + ex.Message, "StatusViewer", MessageBoxButton.OK);
            }
        }


        private void FreezeCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ((MachineState == MachineStateEnum.Running) || (MachineState == MachineStateEnum.Freeze));
        }

        void FreezeCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            System.Windows.Controls.Primitives.ToggleButton tb = e.Source as System.Windows.Controls.Primitives.ToggleButton;
            if (tb.IsChecked.Value)
            {
                MachineState = MachineStateEnum.Freeze;
            }
            else
            {
                MachineState = MachineStateEnum.Running;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public bool AtInitialState
        {
            get { return _machineState == MachineStateEnum.Initial; }
        }
        public MachineStateEnum MachineState
        {
            get
            {
                return _machineState;
            }
            set
            {
                _machineState = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("MachineState"));
                    PropertyChanged(this, new PropertyChangedEventArgs("AtInitialState"));
                }
            }
        }

        public string AppVersion
        {
            get
            {
                var asmb = StaticMethods.GetLibVersion();
                string result = $"StatusViewer. Version {asmb.GetName().Version.ToString(3)}";
                return result;

            }
        }

        public long ReceivedBytes
        {
            get
            {
                return _receivedBytes;
            }

            set
            {
                _receivedBytes = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ReceivedBytes"));
            }
        }


        public long ReceivedDatagrams
        {
            get
            {
                return _receivedDatagrams;
            }

            set
            {
                _receivedDatagrams = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ReceivedDatagrams"));
            }
        }

    }
}
