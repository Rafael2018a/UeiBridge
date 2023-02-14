using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using ByteStreame3.Utilities;
using Newtonsoft;

namespace ByteStreamer3
{

    /// <summary>
    /// Class role: reads json files from given folder, convert them to bytes-block, and sends each block to appropriate udp end point.
    /// </summary>
    class PacketPlayer
    {
        // publics
        public int NumberOfItemsToPlay
        {
            get => _playItemList.Count;
        }
        // privates
        List<PlayItemInfo> _playItemList = new List<PlayItemInfo>();
        System.Threading.CancellationTokenSource _cts = new System.Threading.CancellationTokenSource();
        bool _repeatFlag;
        bool _playOneByOneFlag;
        public PacketPlayer(DirectoryInfo playDir, bool repeatFlag, bool playOneByOneFlag)
        {
            List<FileInfo> inputFilelist = new List<FileInfo>( playDir.GetFiles("*.json"));
            _repeatFlag = repeatFlag;
            _playOneByOneFlag = playOneByOneFlag;

            foreach (FileInfo fi in inputFilelist)
            {
                JsonMessage jm = JsonFileToMessage(fi);
                if (null != jm)
                {
                    _playItemList.Add(new PlayItemInfo(jm));
                }
            }

            // if no file, create sample file.
            if ( 0 == _playItemList.Count)
            {
                string fullname = playDir.FullName + @"\sample.json";
                using (StreamWriter file = File.CreateText( fullname))
                {
                    var jm = new JsonMessage(new JsonMessageHeader(), new JsonMessageBody(new int[] { 0, 1, 2, 3 }));
                    var s = Newtonsoft.Json.JsonConvert.SerializeObject(jm, Newtonsoft.Json.Formatting.Indented);
                    file.Write(s);
                }
            }
        }

        private JsonMessage JsonFileToMessage(FileInfo fi)
        {
            return null;
        }

        //bool PlayOneByOneFlag { get; set; }
        //bool RepeatFlag { get; set; }
        internal List<PlayItemInfo> PlayItemList => _playItemList;

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
        JsonMessage PlayTask2(object o, object o2)
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
                        byte[] block = JsonToByteBlock( item.SourceItem);
                        //System.Threading.Thread.Sleep( item.SourceItem.Header.WaitStateMs);
                        IPAddress ip = IPAddress.Parse( item.SourceItem.Header.DestIp);

                        item.NumberOfPlayedBlocks = 11;
                        //if (null != ip)
                        //{
                        //    IPEndPoint ep = new IPEndPoint( ip, item.SourceItem.Header.DestPort);
                        //    SendUdp(block, ep);
                        //}

                        if (_cts.Token.IsCancellationRequested)
                        {
                            break;
                        }
                    }
                } while (_repeatFlag && (false == _cts.Token.IsCancellationRequested));
            });
        }

        private static void SendUdp(byte[] block, IPEndPoint ep)
        {
            throw new NotImplementedException();
        }

        private static byte[] JsonToByteBlock(JsonMessage jm)
        {
            return null;
        }
    }
    class PlayItemInfo
    {
        public int NumberOfPlayedBlocks { get; set; }
        public JsonMessage SourceItem { get; set; }
        public PlayItemInfo(JsonMessage SourceItem)
        {
            this.SourceItem = SourceItem;
        }
    }

}
