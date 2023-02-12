using System;
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
    public class Rootobject
    {
        public DateTime date { get; set; }
        public string level { get; set; }
        public string logger { get; set; }
        public string message { get; set; }
    }

    public enum MachineStateEnum { Initial, Running, Freeze }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, System.ComponentModel.INotifyPropertyChanged
    {
        System.Net.Sockets.UdpClient m_udpListener;

        ObservableCollection<StatusBaseViewModel> m_entriesList = new ObservableCollection<StatusBaseViewModel>();
        public ObservableCollection<StatusBaseViewModel> EntriesList
        {
            get { return m_entriesList; }
            set { m_entriesList = value; }
        }

        MachineStateEnum _machineState = MachineStateEnum.Initial;
        long _receivedBytes = 0;
        long _receivedDatagrams = 0;

        //Process dbgViewProcess;

        List<string> logDic = new List<string>();

        IPAddress m_multicastIp;
        public IPAddress MulticastIp
        {
            get { return m_multicastIp; }
            set
            {
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("MulticastIp"));

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
        StatusBaseViewModel TryGetValue(ObservableCollection<StatusBaseViewModel> oc, string desc)
        {
            //StatCounter existingStatCounter = null;
            foreach (StatusBaseViewModel sc in EntriesList)
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

            Task.Factory.StartNew(() =>
            {
                for (int i = 0; i < 1000; i++)
                {
                    UdpClient udpClient = new UdpClient();
                    System.Threading.Thread.Sleep(100);

                    StatusEntryJson js = new StatusEntryJson("Test message", new string [] { $"Regular message {i}" }, StatusTrait.IsRegular);
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
                        udpClient.Send(send_buffer1, send_buffer.Length, new IPEndPoint(ip, port));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                }
            });
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
                    SelectedLocalIp = ipa;
                }
            }
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
            freezeCommand.Executed += new ExecutedRoutedEventHandler(freezeCommandExecuted);
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

            switch (messageModel.MessageType)
            {
                case ProjMessageType.Invalid:
                    {
                        AppServices.WriteToTrace(" **** Invalid message ****");
                    }
                    break;

                case ProjMessageType.Text:
                    {
                        // create or update counter entry
                        // ==============================
                        StatusBaseViewModel baseViewModel = TryGetValue(m_entriesList, messageModel.Desc);
                        if (baseViewModel != null) // if object already exist
                        {
                            StatusEntryViewModel vm = baseViewModel as StatusEntryViewModel;
                            vm.Update(messageModel);
                        }
                        else // object not exists. create new one
                        {
                            StatusEntryViewModel vm = new StatusEntryViewModel(messageModel);
                            Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.DataBind, new Action(() => EntriesList.Add(vm)));
                        }
                    }
                    break;

                case ProjMessageType.Counter:
                    {
                        // create or update counter entry
                        // ==============================
                        StatusBaseViewModel baseViewModel = TryGetValue(m_entriesList, messageModel.Desc);
                        if (baseViewModel != null) // if object already exist
                        {
                            StatusCounterViewModel vm = baseViewModel as StatusCounterViewModel;
                            vm.Update(messageModel);
                        }
                        else // object not exists. create new one
                        {
                            StatusCounterViewModel vm = new StatusCounterViewModel(messageModel);
                            Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.DataBind, new Action(() => EntriesList.Add(vm)));
                        }
                    }
                    break;
                //case ProjMessageType.SimpleLog: //entry to emit to DebugView
                //    {
                //        long timeInTicks = Convert.ToInt64(messageModel.ProjTimeInSec * 10000000.0);
                //        TimeSpan ts = new TimeSpan(timeInTicks);

                //        if (false == messageModel.StringValue.StartsWith("=["))
                //        {
                //            string formattedString = string.Format("=#= [{0}; {1}] {2} ", logDic[messageModel.Severity], ts.ToString(@"hh\:mm\:ss"), messageModel.StringValue);
                //            Trace.Write(formattedString);
                //        }
                //        else
                //        {
                //            Trace.Write(messageModel.StringValue);
                //        }
                //    }
                //    break;

            }

            // ********* tbd. change to socket and use ReceiveAsync   !!!!!!!!!!!!!!!!!!

            udpListener.BeginReceive(new AsyncCallback(UdpReceiveCallback), udpState);

        }


        private void StopCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ((MachineState == MachineStateEnum.Running) || (MachineState == MachineStateEnum.Freeze)) ? true : false; // 
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
            ReceivedDatagrams = 0;
        }


        private void StartCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (MachineState == MachineStateEnum.Initial) ? true : false;
        }

        private void StartCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            StartMulticast();
            MachineState = MachineStateEnum.Running;
            StatusCounterViewModel.EnableBindingUpdate = true; // just in case we came here after freeze


        }

        private void StartMulticast()
        {
            IPAddress mcAddress = null;
            //int mcPort = -1;

            
            string mcIp = ConfigurationSettings.AppSettings["multicastIp"];
            mcAddress = (mcIp != null) ? IPAddress.Parse(mcIp) : IPAddress.Parse("239.10.10.17"); // get from config or use default
            string mcPort1 = ConfigurationSettings.AppSettings["multicastPort"];
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
            e.CanExecute = ((MachineState == MachineStateEnum.Running) || (MachineState == MachineStateEnum.Freeze)) ? true : false;
        }

        void freezeCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            System.Windows.Controls.Primitives.ToggleButton tb = e.Source as System.Windows.Controls.Primitives.ToggleButton;
            if (tb.IsChecked.Value)
            {
                MachineState = MachineStateEnum.Freeze;
                StatusCounterViewModel.EnableBindingUpdate = false;
            }
            else
            {
                MachineState = MachineStateEnum.Running;
                StatusCounterViewModel.EnableBindingUpdate = true;
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

        public long ReceivedBytes
        {
            get
            {
                return _receivedBytes;
            }

            set
            {
                _receivedBytes = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("ReceivedBytes"));
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
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("ReceivedDatagrams"));
            }
        }

    }
}
