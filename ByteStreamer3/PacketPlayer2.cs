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
        public bool NowPlaying { get; private set; }
        #endregion

        #region === privates ===
        private DirectoryInfo _playFolder;
        private bool _repeatFlag;
        private bool _playOneByOneFlag;
        //private PacketPlayer _packetPlayer;
        private List<PlayFile> _playItemList;
        System.Threading.CancellationTokenSource _cts = new System.Threading.CancellationTokenSource();
        #endregion

        public PacketPlayer2(DirectoryInfo playFolder, bool repeatFlag, bool playOneByOneFlag)
        {
            NowPlaying = false;
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
        internal Task StartPlay()
        {
            
            //_packetPlayer = new PacketPlayer(_playItemList, _repeatFlag, _playOneByOneFlag);
            //_packetPlayer.StartPlay();
            if (_playOneByOneFlag)
            {
                return StartPlayOneByOne(_playItemList);
            }
            //else
            //{
            //    StartPlaySimultaneously();
            //}

            return null;
        }
        public void StopPlay()
        {
            _cts.Cancel();
        }
        private Task StartPlayOneByOne( List<PlayFile> playList )
        {
            Task t = Task.Factory.StartNew( () =>
            {
               
                try
                {
                    do
                    {

                        // for each file
                        foreach (PlayFile item in playList)
                        {
                            if (!item.IsValidItem)
                                continue;

                            for (int i = 0; i < item.PlayObject.Header.NumberOfCycles; i++)
                            {
                                byte[] block = item.EthMessage.GetByteArray(UeiBridge.Library.MessageWay.downstream);
                                System.Threading.Thread.Sleep(item.PlayObject.Header.WaitStateMs);
                                ++item.PlayedBlockCount;

                                _cts.Token.ThrowIfCancellationRequested();
                            }
                        }
                    } while (_repeatFlag && (false == _cts.Token.IsCancellationRequested));
                }
                finally
                {
                    NowPlaying = false;
                }
            }, _cts.Token);

            return t;
        }

        /// <summary>
        /// Play all items at once
        /// </summary>
        private void StartPlaySimultaneously()
        {
            //Task.Factory.StartNew(() =>
            //{
            //    do
            //    {
            //        foreach (PlayItemInfo jm in _playItemList)
            //        {
            //            byte[] block = JsonToBlock(jm);
            //            System.Threading.Thread.Sleep(jm.Header.WaitStateMs);
            //            IPAddress ip = IPAddress.Parse(jm.Header.DestIp);
            //            if (null != ip)
            //            {
            //                IPEndPoint ep = new IPEndPoint(ip, jm.Header.DestPort);
            //                SendUdp(block, ep);
            //            }

            //            if (_cts.Token.IsCancellationRequested)
            //            {
            //                break;
            //            }
            //        }
            //    } while (repeatFlag && (false == _cts.Token.IsCancellationRequested));
            //});
        }
    }
}
