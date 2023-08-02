using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UeiBridge.Library;

namespace ByteStreamer3
{
    /// <summary>
    /// ViewModel for JFileAux object
    /// </summary>
    class PlayFileViewModel : ViewModelBase
    {
        #region === publics ===
        //public event PropertyChangedEventHandler PropertyChanged;
        public bool IsItemChecked
        {
            get => _isItemChecked;
            set
            {
                _isItemChecked = value;
                PlayFile.JFileObject.Header.EnablePlay = value;
                //RaisePropertyChangedEvent("IsItemChecked");
                RaisePropertyChanged();
            }
        }
        public JFileAux PlayFile { get; private set; }
        public int PlayedBlocksCount
        {
            get => _playedBlocksCount;
            set
            {
                _playedBlocksCount = value;
                //RaisePropertyChangedEvent("PlayedBlocksCount");
                RaisePropertyChanged();
            }
        }
        public string Filename { get; set; }
        public string FixedDesc { get; private set; }
        public string EntryToolTip
        {
            get => _entryTooltip;
            set
            {
                _entryTooltip = value;
                RaisePropertyChanged();
            }
        }
        public int NoOfCycles => PlayFile.JFileObject.Header.NumberOfCycles;
        #endregion

        #region === privates ===
        private string _entryTooltip;
        private int _playedBlocksCount;
        private bool _isItemChecked = true;
        #endregion

        //void RaisePropertyChangedEvent(string propName)
        //{
        //    if (PropertyChanged != null)
        //        PropertyChanged(this, new PropertyChangedEventArgs(propName));
        //}

        public PlayFileViewModel(JFileAux playFile)
        {
            this.PlayFile = playFile;
            Filename = PlayFile.PlayFileInfo.Name;

            var ip = System.Net.IPAddress.Parse(PlayFile.JFileObject.Header.DestIp);
            var destEp = new System.Net.IPEndPoint(ip, PlayFile.JFileObject.Header.DestPort);

            FixedDesc = $"Dest: {destEp}";

            string name = DeviceMap2.GetDeviceName(playFile.JFileObject.Body.CardType);
            _entryTooltip = $"Device: {name} ({playFile.JFileObject.Body.CardType}) Slot:{playFile.JFileObject.Body.SlotNumber} Cube:{playFile.JFileObject.Body.CubeId}";

            IsItemChecked = playFile.JFileObject.Header.EnablePlay;
        }

    }
}
