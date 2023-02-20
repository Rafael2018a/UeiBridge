using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using ByteStreame3.Utilities;
using Newtonsoft;
using System.Collections.ObjectModel;
using Newtonsoft.Json;

namespace ByteStreamer3
{

    /// <summary>
    /// Class role: reads json files from given folder, convert them to bytes-block, and sends each block to appropriate udp end point.
    /// </summary>
    class PacketPlayer
    {
        #region === publics ===
        public int NumberOfItemsToPlay => _playItemList.Count;
        internal List<PlayItemInfo> PlayItemList => _playItemList;

        public ObservableCollection<PlayItemInfo> NowPlayingList { get; internal set; }
        #endregion
        #region === privates ====
        List<PlayItemInfo> _playItemList = new List<PlayItemInfo>();
        System.Threading.CancellationTokenSource _cts = new System.Threading.CancellationTokenSource();
        bool _repeatFlag;
        bool _playOneByOneFlag;
        DirectoryInfo _playFolder;
        #endregion


        public void SetPlayFolder(DirectoryInfo playFolder)
        {
            _playFolder = playFolder;
            NowPlayingList = BuildNowPlayingList(playFolder);
        }
        public PacketPlayer(DirectoryInfo playFolder, bool repeatFlag, bool playOneByOneFlag)
        {
            _playFolder = playFolder;
            _repeatFlag = repeatFlag;
            _playOneByOneFlag = playOneByOneFlag;

            NowPlayingList = BuildNowPlayingList(playFolder);

            //foreach (FileInfo fileToPlay in palyFilelist)
            //{
            //    //PlayItemJson jm = JsonFileToMessage(fileToPlay);
            //    PlayItemInfo itemInfo = new PlayItemInfo(fileToPlay);
            //    itemInfo.ByteBlock = ConvertPlayItemToBytes(itemInfo.JsonItem);
            //    if (null != itemInfo.ByteBlock)
            //    {
            //        _playItemList.Add(itemInfo);
            //    }
            //}

            // if no file, create sample file.
            if ( 0 == _playItemList.Count)
            {
                string fullname = "example.json";
                using (StreamWriter file = File.CreateText( fullname))
                {
                    var jm = new PlayItemJson(new ItemHeader(), new ItemBody(new int[] { 0, 1, 2, 3 }));
                    var s = JsonConvert.SerializeObject(jm, Newtonsoft.Json.Formatting.Indented);
                    file.Write(s);
                }
            }
        }

        private ObservableCollection<PlayItemInfo> BuildNowPlayingList(DirectoryInfo playFolder)
        {
            List<FileInfo> l1 = GetValidJsonFilesList(playFolder);
            ObservableCollection<PlayItemInfo> resultList;
            if (null != l1)
            {
                var l2 = l1.Select(i => new PlayItemInfo(i));
                resultList = new ObservableCollection<PlayItemInfo>(l2);
            }
            else
            {
                resultList = new ObservableCollection<PlayItemInfo>();
            }
            return resultList;
        }

        /// <summary>
        /// Get list of json's from current dir.
        /// Only files with compatible json struct
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        List<FileInfo> GetValidJsonFilesList(DirectoryInfo folder)
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
                        var js = JsonConvert.DeserializeObject<PlayItemJson>(reader.ReadToEnd());

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

        private byte[] ConvertPlayItemToBytes(PlayItemJson jsonItem)
        {
            throw new NotImplementedException();
        }

        private PlayItemJson JsonFileToMessage(FileInfo fi)
        {
            return null;
        }

        //bool PlayOneByOneFlag { get; set; }
        //bool RepeatFlag { get; set; }

        internal void StartPlay()
        {
            if (_playOneByOneFlag)
            {
                StartPlayOneByOne();
            }
            else
            {
                StartPlaySimultaneously();
            }
        }

        internal void Stop()
        {
            _cts.Cancel();
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

        void PlayTask(object o)
        {
            
        }
        PlayItemJson PlayTask2(object o, object o2)
        {
            return null;
        }
        //static void PlayOneByOneTask(List<JsonMessage> jsonItems, bool repeatFlag, System.Threading.CancellationToken token)
        //{
        //    do
        //    {
        //        foreach (JsonMessage jm in jsonItems)
        //        {
        //            byte[] block = JsonToBlock(jm);
        //            System.Threading.Thread.Sleep(jm.Header.WaitStateMs);
        //            IPAddress ip = IPAddress.Parse(jm.Header.DestIp);
        //            if (null != ip)
        //            {
        //                IPEndPoint ep = new IPEndPoint(ip, jm.Header.DestPort);
        //                SendUdp(block, ep);
        //            }

        //            if ( token.IsCancellationRequested)
        //            {
        //                break;
        //            }
        //        }
        //    } while (repeatFlag && (false == token.IsCancellationRequested));
        //}

        private void StartPlayOneByOne()
        {

            Task.Factory.StartNew(() => 
            {
                do
                {
                    // for each file
                    foreach (PlayItemInfo item in _playItemList)
                    {
                        //byte[] block = JsonToByteBlock( item.SourceItem);
                        ////System.Threading.Thread.Sleep( item.SourceItem.Header.WaitStateMs);
                        //IPAddress ip = IPAddress.Parse( item.SourceItem.Header.DestIp);

                        //item.NumberOfPlayedBlocks = 11;
                        ////if (null != ip)
                        ////{
                        ////    IPEndPoint ep = new IPEndPoint( ip, item.SourceItem.Header.DestPort);
                        ////    SendUdp(block, ep);
                        ////}

                        //if (_cts.Token.IsCancellationRequested)
                        //{
                        //    break;
                        //}
                    }
                } while (_repeatFlag && (false == _cts.Token.IsCancellationRequested));
            });
        }

        private static void SendUdp(byte[] block, IPEndPoint ep)
        {
            throw new NotImplementedException();
        }

        private static byte[] JsonToByteBlock(PlayItemJson jm)
        {
            return null;
        }
    }
    class PlayItemInfo
    {
        string _name = "name1";
        int _playedBlocks = 19;
        #region == public ==
        public string Name { get => _name; set => _name = value; }
        public int PlayedBlocks { get => _playedBlocks; set => _playedBlocks = value; }
        #endregion
        FileInfo filename;
        private FileInfo fileToPlay;
        public byte[] ByteBlock;
        public int NumberOfPlayedBlocks { get; set; }
        public PlayItemJson JsonItem { get; set; }
        


        //public PlayItemInfo(PlayItemJson jsonItem)
        //{
        //    this.JsonItem = jsonItem;
        //}
        public PlayItemInfo(FileInfo fileToPlay)
        {
            this.fileToPlay = fileToPlay;
        }
    }
}
