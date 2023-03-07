using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ByteStreamer3.Utilities;


namespace ByteStreamer3
{
    class PacketPlayer2 //: INotifyPropertyChanged
    {
        #region === publics ====
        public ObservableCollection<PlayFileViewModel> PlayList { get; internal set; }
        //public event PropertyChangedEventHandler PropertyChanged;
        #endregion
        #region === privates ===
        private DirectoryInfo _playFolder;
        private bool _repeatFlag;
        private bool _playOneByOneFlag;
        private PacketPlayer _packetPlayer;
        private List<PlayFile> _playItemList;
        #endregion
        public PacketPlayer2(DirectoryInfo playFolder, bool repeatFlag, bool playOneByOneFlag)
        {
            this._playFolder = playFolder;
            this._repeatFlag = repeatFlag;
            this._playOneByOneFlag = playOneByOneFlag;

            SetPlayFolder(playFolder);
        }
        public void SetPlayFolder(DirectoryInfo playFolder)
        {
            if (!playFolder.Exists)
                return;
            this._playFolder = playFolder;
            FileInfo[] jsonlist = playFolder.GetFiles("*.json");
            _playItemList = new List<PlayFile>( jsonlist.Select(i => new PlayFile(i)).Where(i => i.IsValidItem));
            var vmlist = _playItemList.Select(i => new PlayFileViewModel( i));
            PlayList = new ObservableCollection<PlayFileViewModel>( vmlist);
        }
        private List<PlayFile> BuildPlayList(DirectoryInfo playFolder)
        {
            List<FileInfo> l1 = ReadJsonFilesList(playFolder);
            if (null != l1)
            {
                return new List<PlayFile>(l1.Select(i => new PlayFile(i)));
            }
            else
            {
                return new List<PlayFile>();
            }
        }
        /// <summary>
        /// Get list of json's from current dir.
        /// Only files with compatible json struct
        /// </summary>
        List<FileInfo> ReadJsonFilesList(DirectoryInfo folder)
        {
            if (!folder.Exists)
                return null;

            FileInfo[] list = folder.GetFiles("*.json");
            List<FileInfo> result = new List<FileInfo>();
            foreach (FileInfo jfile in list)
            {
                using (StreamReader reader = jfile.OpenText())
                {
                    try
                    {
                        var js = JsonConvert.DeserializeObject<JItem>(reader.ReadToEnd());
                        if (null != js && null != js.Header)
                        {
                            result.Add(jfile);
                        }
                    }
                    catch (JsonSerializationException ex)
                    {

                    }
                }
            }
            return result;
        }
        //void RaisePropertyChangedEvent(string propName)
        //{
        //    if (PropertyChanged != null)
        //        PropertyChanged(this, new PropertyChangedEventArgs(propName));
        //}

        internal void StartPlay()
        {
            _packetPlayer = new PacketPlayer(_playItemList, _repeatFlag, _playOneByOneFlag);
            _packetPlayer.StartPlay();

        }
    }
}
