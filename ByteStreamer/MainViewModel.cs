using System;
using System.Windows;
using System.ComponentModel;
using System.Net;
using System.Collections.Generic;
using ByteStreamer.Utilities;


namespace ByteStreamer
{
    public class MainViewModel : System.ComponentModel.INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        SettingBag _settings;

        IpPlayer player;
        System.Windows.Threading.DispatcherTimer _rateUpdateTimer = new System.Windows.Threading.DispatcherTimer();
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        MovingAverageFloat maBps = new MovingAverageFloat(10);
        //MovingAverageFloat desiredMaKbps = new MovingAverageFloat(10);
        int _delayVectorLength = 1000;
        //byte[] delayVector = new byte[_delayVectorLength];
        System.Threading.Timer _delayVectorUpdateTimer;
        //int _delayVectorFilledCells = 0;
        DelayVector _dv;
        string _settingsFilename = "bytestreamer.setting.bin";


        public MainViewModel()
        {
            LoadCommands();
            _dv = new DelayVector(_delayVectorLength);
            _settings = LoadSetting();
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

            //if (o != null)
            //    return sb;
            //else
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
        double _playRateMbps;
        public double PlayRate // mbit/sec
        {
            get => _playRateMbps;
            set
            {
                _playRateMbps = value;
                RaisePropertyChangedEvent("PlayRate");
            }
        }
        double _desiredPlayRateMbps;
        public double DesiredRate
        {
            get => _desiredPlayRateMbps;
            set
            {
                _desiredPlayRateMbps = value;
                RaisePropertyChangedEvent("DesiredRate");
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
        //public Utilities.RelayCommand StartPlayCommand1 { get; set; }
        //public Utilities.RelayCommand StopPlayCommand1 { get; set; }

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

            _delayVectorUpdateTimer = new System.Threading.Timer(new System.Threading.TimerCallback(OnDelayVectorUpdateTick), null, 1000, 1000);

            player = new IpPlayer(destEp);
            byte[] block = new byte[BlockLength];
            player.StartPlayAsync2(block, TimeSpan.FromMilliseconds(_settings.waitStatesMS), _dv).ContinueWith((t) => { IsPlaying = false; });

            StartPlayCommand.OnCanExecuteChanged();

            sw.Start();
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
            PlayRate = 0;
            DesiredRate = 0;
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
                //desiredBytes = player.DesiredBytesCount;
                //player.DesiredBytesCount = 0;
            }
            double sec = Convert.ToDouble( sw.ElapsedMilliseconds / 1000.0);
            
            sw.Restart();

            if (sec > 0)
            {
                double bps = 8.0 * playedBytes / sec;
                maBps.AddItem(bps);
                PlayRate = maBps.Average / 1000000.0;

                //int dkbps = Convert.ToInt32(8 * desiredBytes / ms);
                //desiredMaKbps.AddItem(dkbps);
                //DesiredRate = desiredMaKbps.Average / 1000.0;
            }

        }

        void OnDelayVectorUpdateTick(object obj)
        {
            double delayPercent = 100.0 - RatePercent;
            _dv.SetFillPercent(delayPercent);
        }

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
