using System;
using System.Windows;
using System.ComponentModel;
using System.Net;
using System.Collections.Generic;
using System.IO;
using Newtonsoft;
using Newtonsoft.Json;
using ByteStreamer.Utilities;


namespace ByteStreamer
{
    public class MainViewModel : System.ComponentModel.INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        SettingBag _settings;
        IpPlayer player;
        System.Windows.Threading.DispatcherTimer _rateUpdateTimer = new System.Windows.Threading.DispatcherTimer();
        System.Diagnostics.Stopwatch _stopWatch = new System.Diagnostics.Stopwatch();
        MovingAverageFloat _mvAvgBitPerSec = new MovingAverageFloat(10);
        string _settingsFilename = "bytestreamer.setting.bin";

        public MainViewModel()
        {
            LoadCommands();
            //_dv = new DelayVector(_delayVectorLength);
            _settings = LoadSetting();

            // save default file (for example)
            JsonMessageHeader mh = new JsonMessageHeader();
            JsonMessageBody mb = new JsonMessageBody(new int[] { 11, 21, 22 });
            JsonMessage jm = new JsonMessage(mh, mb);
            string jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(jm, Formatting.Indented);
            using (StreamWriter file = File.CreateText("MessagePrototype.json"))
            {
                file.WriteLine(jsonString);
            }

            // show number of message files
            var x = Directory.GetFiles(".", "*.json");
            _numberOfFilesInFolder = x.Length;
        }

        ~MainViewModel()
        {
            SaveSetting(_settings);
        }
        private SettingBag LoadSetting()
        {
            System.Runtime.Serialization.IFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            SettingBag sb;
            try
            {
                System.IO.FileStream fs = new System.IO.FileStream(_settingsFilename, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                object o = formatter.Deserialize(fs);
                sb = o as SettingBag;
            }
            catch(Exception ex)
            {
                sb = new SettingBag();
            }

            return sb;
        }

        void SaveSetting(SettingBag setting)
        {
            System.Runtime.Serialization.IFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            System.IO.FileStream fs = new System.IO.FileStream(_settingsFilename, System.IO.FileMode.Create, System.IO.FileAccess.Write);
            formatter.Serialize( fs, setting);
            fs.Close();
        }

        void RaisePropertyChangedEvent(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        #region BindProperties =====
        public string DestinationIp
        {
            get => _settings.destinationIp;
            set
            {
                _settings.destinationIp = value;
                RaisePropertyChangedEvent("DestinationIp");
            }
        }
        public int DestinationPort
        {
            get { return _settings.destinationPort; }
            set
            {
                _settings.destinationPort = value;
                RaisePropertyChangedEvent("DestinationPort");
            }
        }
        public int BlockLength
        {
            get => _settings.blockLength;
            set
            {
                _settings.blockLength = value;
                RaisePropertyChangedEvent("BlockLength");
            }
        }
        public double RatePercent
        {
            get => _settings.ratePercent;
            set
            {
                if (value <= 100.0)
                {
                    _settings.ratePercent = value;
                    RaisePropertyChangedEvent("RatePercent");
                }
            }
        }
        int _numberOfFilesInFolder;
        public int NumberOfFilesInFolder
        {
            get => _numberOfFilesInFolder;
            set
            {
                _numberOfFilesInFolder = value;
                RaisePropertyChangedEvent("NumberOfFilesInFolder");
            }
        }
        public long PlayedBytesCount
        {
            get
            {
                if (player == null)
                {
                    return 0;
                }
                else
                {
                    return player.PlayedBytesCount;
                }
            }
        }

        float _playRateKbitPerSec;
        public string PlayRateKbitPerSec 
        {
            get => "100";//_playRateKbitPerSec;
            set
            {
                _playRateKbitPerSec = float.Parse( value);
                RaisePropertyChangedEvent("PlayRateKbitPerSec");
            }
        }
        JsonMessageHeader _nowPlayingFile;
        public JsonMessageHeader NowPlayingHeader
        {
            get => _nowPlayingFile;
            set 
            {
                _nowPlayingFile = value;
                RaisePropertyChangedEvent("NowPlayingHeader");
            }
        }
        bool _isPlaying = false;
        public bool IsPlaying
        {
            get => _isPlaying;
            set
            {
                _isPlaying = value;
                RaisePropertyChangedEvent("IsPlaying");
            }
        }
        public int WaitStatesMS
        {
            get => _settings.waitStatesMS;
            set
            {
                _settings.waitStatesMS = value;
                RaisePropertyChangedEvent("WaitStatesMS");
            }
        }
        #endregion

        #region ICommands =====
        private Utilities.RelayCommand _startPlayCommand;
        public Utilities.RelayCommand StartPlayCommand
        {
            get => _startPlayCommand;
            set => _startPlayCommand = value;
        }
        private Utilities.RelayCommand _stopPlayCommand;
        public Utilities.RelayCommand StopPlayCommand
        {
            get { return _stopPlayCommand; }
            set { _stopPlayCommand = value; }
        }


        private void LoadCommands()
        {
            StartPlayCommand = new ByteStreamer.Utilities.RelayCommand(StartPlay, CanStartPlay);
            StopPlayCommand = new Utilities.RelayCommand(StopPlay, CanStopPlay);
        }
        public static List<byte[]> Make_SL508Down_Messages(int seed)
        {
            List<byte[]> msgs = new List<byte[]>();

            // build 8 messages, one per channel
            for (int ch = 0; ch < 8; ch++)
            //int ch = 1;
            {
                string m = $"hello ch{ch} seed {seed} ksd klskd kljasldkjf laksjdfkl klsjd fkasdfjlk askldjfklasjdf asdfklj ksdajf ";

                // string to ascii

                // ascii to string System.Text.Encoding.ASCII.GetString(recvBytes)
                UeiBridge.EthernetMessage msg = UeiBridge.EthernetMessageFactory.CreateEmpty(5, 16);
                msg.PayloadBytes = System.Text.Encoding.ASCII.GetBytes(m);
                msg.SlotChannelNumber = ch;
                msgs.Add(msg.ToByteArrayDown());

            }

            return msgs;

        }

        void StartPlay(object obj)
        {
            IPEndPoint destEp;
            try
            {
                destEp = new IPEndPoint( IPAddress.Parse(DestinationIp), DestinationPort);
            }
            catch(Exception ex)
            {
                MessageBox.Show( ex.Message, "Invalid IP/Port Error", MessageBoxButton.OK);
                return;
            }

            IsPlaying = true;
            _rateUpdateTimer.Tick += new EventHandler(OnRateTimerTick);
            _rateUpdateTimer.Interval = TimeSpan.FromMilliseconds(1000);
            _rateUpdateTimer.Start();

            //_delayVectorUpdateTimer = new System.Threading.Timer(new System.Threading.TimerCallback(OnDelayVectorUpdateTick), null, 1000, 1000);

            player = new IpPlayer(destEp);
            //byte[] block = new byte[BlockLength];
            //player.StartPlayAsync2(block, TimeSpan.FromMilliseconds(_settings.waitStatesMS), _dv).ContinueWith((t) => { IsPlaying = false; });
            List<byte[]> blockList = Make_SL508Down_Messages(10);
            //player.StartPlayAsync3(blockList, TimeSpan.FromMilliseconds(_settings.waitStatesMS)).ContinueWith((t) => { IsPlaying = false; });
            player.StartPlayAsync4(".");



            StartPlayCommand.OnCanExecuteChanged();

            _stopWatch.Start();
        }

        bool CanStartPlay(object obj)
        {
            return (IsPlaying == false);
        }
        void StopPlay(object obj)
        {
            //_stopPlayRequest = true;
            player.AbortPlay();
            IsPlaying = false;
            StopPlayCommand.OnCanExecuteChanged();
            _rateUpdateTimer.Stop();
            PlayRateKbitPerSec = "";
            //DesiredRate = 0;
        }
        bool CanStopPlay(object obj)
        {
            return IsPlaying;
        }
        #endregion


        #region callbacks
        private void OnRateTimerTick(object sender, EventArgs e)
        {
            long playedBytes;
            //long desiredBytes;
            lock (player.CountLock)
            {
                playedBytes = player.PlayedBytesCount;
                player.PlayedBytesCount = 0;
                NowPlayingHeader = player.NowPlayingHeader;
                //desiredBytes = player.DesiredBytesCount;
                //player.DesiredBytesCount = 0;
            }
            double sec = Convert.ToDouble( _stopWatch.ElapsedMilliseconds / 1000.0);
            
            _stopWatch.Restart();

            if (sec > 0)
            {
                double bps = 8.0 * playedBytes / sec;
                _mvAvgBitPerSec.AddItem(bps);
                PlayRateKbitPerSec = (_mvAvgBitPerSec.Average / 1000.0).ToString();

                //int dkbps = Convert.ToInt32(8 * desiredBytes / ms);
                //desiredMaKbps.AddItem(dkbps);
                //DesiredRate = desiredMaKbps.Average / 1000.0;
            }

        }

        //void OnDelayVectorUpdateTick(object obj)
        //{
        //    double delayPercent = 100.0 - RatePercent;
        //    _dv.SetFillPercent(delayPercent);
        //}

        //    void OnDelayVectorUpdateTick1(object obj)
        //{
        //    double delayPercent = 100.0 - RatePercent;
        //    Int32 numberOfCellsToFill = Convert.ToInt32(delayPercent * delayVector.Length / 100.0);
        //    Random rdm = new Random();
        //    while (numberOfCellsToFill > _delayVectorFilledCells)
        //    {
        //        int r = rdm.Next(delayVector.Length - 1);
        //        if (delayVector[r] == 0)
        //        {
        //            delayVector[r] = 1;
        //            _delayVectorFilledCells++;
        //        }
        //    }

        //    while (numberOfCellsToFill < _delayVectorFilledCells)
        //    {
        //        int r = rdm.Next(delayVector.Length - 1);
        //        if (delayVector[r] == 1)
        //        {
        //            delayVector[r] = 0;
        //            _delayVectorFilledCells--;
        //        }
        //    }
        //}

        //List<int> _filledCells = new List<int>();
        //List<int> _emptyCells = new List<int>();

        //void UpdateDelayVector( double desiredPercent)
        //{
        //    double delayPercent = 100.0 - RatePercent;
        //    Int32 numberOfCellsToFill = Convert.ToInt32(delayPercent * delayVector.Length / 100.0);
        //    Random rdm = new Random();
        //    if (numberOfCellsToFill > _filledCells.Count)
        //    {
        //        int r = rdm.Next(_emptyCells.Count);
        //        int cellNumber = _emptyCells[r];
        //        _emptyCells.RemoveAt(r);
        //        _filledCells.Add(cellNumber);
        //    }
        //}

        #endregion
    }
}
