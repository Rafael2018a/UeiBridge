using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using ByteStreamer.Utilities;

namespace ByteStreamer
{
    /// <summary>
    /// Plays a given data-block in udp.
    /// </summary>
    class IpPlayer
    {
        IPEndPoint _remoteEndpoint;
        bool _abortPlayRequest = false;
        long _playedBytesCount = 0;
        public long PlayedBytesCount { get => _playedBytesCount; set => _playedBytesCount = value; }
        //long _desiredBytesCount = 0;
        //public long DesiredBytesCount { get => _desiredBytesCount; set => _desiredBytesCount = value; }
        object _countLock = new object();
        public object CountLock { get => _countLock; }
        

        public IpPlayer(IPEndPoint destEndpoint)
        {
            _remoteEndpoint = destEndpoint;
        }

        public Task StartPlayAsync2(byte[] bytesBlock, TimeSpan waitState, DelayVector _dv)
        {
            Task task1 = Task.Run(() =>
            {
                UdpClient client = new UdpClient();
                client.Connect(_remoteEndpoint);
                int cycleCounter = 0;

                while (true)
                {
                    int sentBytes = client.Send(bytesBlock, bytesBlock.Length);
                    if (_dv[cycleCounter] > 0)
                    {
                        System.Threading.Thread.Sleep(waitState);
                    }
                    cycleCounter = (cycleCounter + 1) % _dv.Length;

                    lock (CountLock)
                    {
                        _playedBytesCount += sentBytes;
                        //_desiredBytesCount += bytesBlock.Length;
                    }

                    if (_abortPlayRequest)
                    {
                        _abortPlayRequest = false;
                        break;
                    }
                }
            }
                );

            return task1;
        }


        public void AbortPlay()
        {
            _abortPlayRequest = true;
        }

    }
}
