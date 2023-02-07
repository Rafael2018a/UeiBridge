using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using ByteStreame3.Utilities;

namespace ByteStreamer3
{
    /// <summary>
    /// This class represent all data from json file
    /// </summary>
    //class PlayItemInfo
    //{
    //    string SourceFileFullname { get; set; }
    //    string _title;
    //    IPEndPoint _destEP;
    //    TimeSpan _waitState;
    //    int _blockLength;
    //    string _converterName;
    //}
    class PlayItemState
    {
        int NumberOfPlayedBlocks;
    }
    /// <summary>
    /// Class role: reads json files from given folder, convert them to bytes-block, and sends each block to appropriate udp end point.
    /// </summary>
    class PacketPlayer
    {
        // publics
        public int NumberOfItemsToPlay
        {
            get => _jsonItemList.Count;
        }

        // privates

        List<JsonMessage> _jsonItemList = new List<JsonMessage>();
        DirectoryInfo _playFolder;
        List<FileInfo> _inputFilelist;
        System.Threading.CancellationTokenSource _cts = new System.Threading.CancellationTokenSource();

        public PacketPlayer(DirectoryInfo di)
        {
            this._playFolder = di;
            _inputFilelist = new List<FileInfo>(_playFolder.GetFiles("*.json"));
            foreach (FileInfo fi in _inputFilelist)
            {
                JsonMessage jm = JsonFileToMessage(fi);
                if (null != jm)
                {
                    _jsonItemList.Add(jm);
                }
            }
        }

        private JsonMessage JsonFileToMessage(FileInfo fi)
        {
            throw new NotImplementedException();
        }

        bool PlayOneByOneFlag { get; set; }
        bool RepeatFlag { get; set; }
        internal void Start()
        {
            if (PlayOneByOneFlag)
            {
                StartPlayOneByOne(_jsonItemList, RepeatFlag);
            }
            else
            {
                StartPlaySimultaneously(_jsonItemList, RepeatFlag);
            }
        }

        internal void Stop()
        {
            _cts.Cancel();
        }

        /// <summary>
        /// Play all items at once
        /// </summary>
        private void StartPlaySimultaneously(List<JsonMessage> jsonItems, bool repeatFlag)
        {
            Task.Factory.StartNew(() =>
            {
                do
                {
                    foreach (JsonMessage jm in jsonItems)
                    {
                        byte[] block = JsonToBlock(jm);
                        System.Threading.Thread.Sleep(jm.Header.WaitStateMs);
                        IPAddress ip = IPAddress.Parse(jm.Header.DestIp);
                        if (null != ip)
                        {
                            IPEndPoint ep = new IPEndPoint(ip, jm.Header.DestPort);
                            SendUdp(block, ep);
                        }

                        if (_cts.Token.IsCancellationRequested)
                        {
                            break;
                        }
                    }
                } while (repeatFlag && (false == _cts.Token.IsCancellationRequested));
            });
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

        private void StartPlayOneByOne(List<JsonMessage> jsonItems, bool repeatFlag)
        {
            //Task.Factory.StartNew(() => PlayOneByOneTask(jsonItems, repeatFlag, _cts.Token));

            Task.Factory.StartNew(() => 
            {
                do
                {
                    foreach (JsonMessage jm in jsonItems)
                    {
                        byte[] block = JsonToBlock(jm);
                        System.Threading.Thread.Sleep(jm.Header.WaitStateMs);
                        IPAddress ip = IPAddress.Parse(jm.Header.DestIp);
                        if (null != ip)
                        {
                            IPEndPoint ep = new IPEndPoint(ip, jm.Header.DestPort);
                            SendUdp(block, ep);
                        }

                        if (_cts.Token.IsCancellationRequested)
                        {
                            break;
                        }
                    }
                } while (repeatFlag && (false == _cts.Token.IsCancellationRequested));
            });
        }

        private static void SendUdp(byte[] block, IPEndPoint ep)
        {
            throw new NotImplementedException();
        }

        private static byte[] JsonToBlock(JsonMessage jm)
        {
            throw new NotImplementedException();
        }
    }
}
