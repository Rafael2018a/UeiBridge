using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ByteStreamer3
{
    class PlayFileViewModel: INotifyPropertyChanged
    {
        #region === publics ===
        public event PropertyChangedEventHandler PropertyChanged;
        public bool IsItemChecked { get; set; } = true;
        public PlayFile PlayFile { get => _playFile; set => _playFile = value; }
        public int PlayedBlocksCount
        {
            get => _playedBlocksCount;
            set
            {
                _playedBlocksCount = value;
                VarDesc = "Played blocks count: " + value.ToString();
            }
        }
        public string Filename { get; set; }
        public string FixedDesc { get => _fixedDesc; set => _fixedDesc = value; }
        public string VarDesc
        {
            get => _varDesc;
            set
            {
                _varDesc = value;
                RaisePropertyChangedEvent("VarDesc");
            }
        }
        #endregion

        #region === privates ===
        string _fixedDesc;
        string _varDesc;
        int _playedBlocksCount;
        PlayFile _playFile;
        #endregion

        void RaisePropertyChangedEvent(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        public PlayFileViewModel( PlayFile playFile)
        {
            this._playFile = playFile;
            Filename = _playFile.PlayFileInfo.Name;
             _fixedDesc =  " Dest: " + _playFile.DestEndPoint.ToString();
        }
    }
}
