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
        public event PropertyChangedEventHandler PropertyChanged;
        public string Name => _playItem.PlayFileInfo.Name + _playItem.PlayedBlockCount.ToString();
        public int PlayedBlocksCount => _playItem.PlayedBlockCount;

        PlayFile _playItem;
        void RaisePropertyChangedEvent(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        public PlayFileViewModel( PlayFile playItem)
        {
            _playItem = playItem;
            _playItem.PlayedBlockCountEvent += _playItem_PlayedBlockCountEvent;
        }

        private void _playItem_PlayedBlockCountEvent(int obj)
        {
            RaisePropertyChangedEvent("Name");
        }
    }
}
