using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace ByteStreamer3
{
    class PlayItem
    {
        string _filename;
        string _blockTitle;
        IPEndPoint _destEP;
        TimeSpan _waitState;
        int _blockLength;
        string _converterName;
        int _baudrate; // bps
    }
    /// <summary>
    /// Class role: reads json files from given folder, convert them to bytes-block, and sends each block to appropriate udp end point.
    /// </summary>
    class PacketPlayer
    {
        PlayItem _currentPlayItem;
    }
}
