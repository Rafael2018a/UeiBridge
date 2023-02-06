using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;

namespace ByteStreamer3
{
    class PlayItem
    {
        string SourceFileFullname { get; set; }
        string _title;
        IPEndPoint _destEP;
        TimeSpan _waitState;
        int _blockLength;
        string _converterName;
        //int _baudrate; // bps
    }
    /// <summary>
    /// Class role: reads json files from given folder, convert them to bytes-block, and sends each block to appropriate udp end point.
    /// </summary>
    class PacketPlayer
    {
        PlayItem _currentPlayItem;
        private DirectoryInfo _playFolder;
        List<FileInfo> _filelist;

        public PacketPlayer(DirectoryInfo di)
        {
            this._playFolder = di;
            _filelist = new List<FileInfo>( _playFolder.GetFiles("*.json"));
        }
        bool PlayOneByOne { get; set; }
        bool RepeatFlag { get; set; }
        internal void Start()
        {
            if (PlayOneByOne)
            {
                StartPlayOneByOne(_filelist);
            }
            else
            {
                StartPlaySimultaneously(_filelist);
            }
        }

        private void StartPlaySimultaneously(List<FileInfo> filelist)
        {
            throw new NotImplementedException();
        }

        private void StartPlayOneByOne(List<FileInfo> filelist)
        {
            throw new NotImplementedException();
        }
    }
}
