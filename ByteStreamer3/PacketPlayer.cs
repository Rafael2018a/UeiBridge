using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using ByteStreamer3.Utilities;
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
        internal List<PlayFile> PlayList => _playList;
        #endregion
        #region === privates ====
        List<PlayFile> _playList;
        System.Threading.CancellationTokenSource _cts = new System.Threading.CancellationTokenSource();
        bool _repeatFlag;
        bool _playOneByOneFlag;
        //DirectoryInfo _playFolder;
        #endregion

        public PacketPlayer(List<PlayFile> playList, bool repeatFlag, bool playOneByOneFlag)
        {
            //_playFolder = playFolder;
            _playList = playList;
            _repeatFlag = repeatFlag;
            _playOneByOneFlag = playOneByOneFlag;

            //_playList = BuildPlayList(playFolder);

            // if no file, create sample file.
            if ( 0 == _playList.Count)
            {
                string fullname = "example.json";
                using (StreamWriter file = File.CreateText( fullname))
                {
                    var jm = new JItem(new JItemHeader(), new JItemBody(new int[] { 0, 1, 2, 3 }));
                    var s = JsonConvert.SerializeObject(jm, Newtonsoft.Json.Formatting.Indented);
                    file.Write(s);
                }
            }
        }


        private byte[] ConvertPlayItemToBytes(JItem jsonItem)
        {
            throw new NotImplementedException();
        }

        private JItem JsonFileToMessage(FileInfo fi)
        {
            return null;
        }

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
        JItem PlayTask2(object o, object o2)
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
                    foreach (PlayFile item in _playList)
                    {
                        if (!item.IsValidItem)
                            continue;

                        for (int i = 0; i < item.PlayObject.Header.NumberOfCycles; i++)
                        {
                            byte[] block = item.EthMessage.GetByteArray(UeiBridge.Library.MessageWay.downstream);
                            System.Threading.Thread.Sleep(item.PlayObject.Header.WaitStateMs);
                            ++item.PlayedBlockCount;

                            //Console.WriteLine($"{item.PlayFile.Name} # {item.NumberOfPlayedBlocks}");
                        }
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

        //private static byte[] JsonToByteBlock(PlayItemJson jm)
        //{
        //    return null;
        //}
    }
}
