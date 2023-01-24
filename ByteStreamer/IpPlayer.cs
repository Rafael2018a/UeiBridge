using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using ByteStreamer.Utilities;
using System.Windows;
using System.IO;
using Newtonsoft;
using Newtonsoft.Json;

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
        JsonMessageHeader _nowPlayingHeader;
        public long PlayedBytesCount { get => _playedBytesCount; set => _playedBytesCount = value; }
        //long _desiredBytesCount = 0;
        //public long DesiredBytesCount { get => _desiredBytesCount; set => _desiredBytesCount = value; }
        object _countLock = new object();
        public object CountLock { get => _countLock; }
        internal JsonMessageHeader NowPlayingHeader { get => _nowPlayingHeader; set => _nowPlayingHeader = value; }

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
        public Task StartPlayAsync3(List<byte[]> bytesBlockList, TimeSpan waitState)
        {
            Task task1 = Task.Run(() =>
            {
                try
                {

                    UdpClient client = new UdpClient();
                    client.Connect(_remoteEndpoint);

                    while (true)
                    {
                        foreach (byte[] bytesBlock in bytesBlockList)
                        {
                            int sentBytes = client.Send(bytesBlock, bytesBlock.Length);
                            System.Threading.Thread.Sleep(waitState);

                            lock (CountLock)
                            {
                                _playedBytesCount += sentBytes;
                            }
                        }
                        if (_abortPlayRequest)
                        {
                            _abortPlayRequest = false;
                            break;
                        }
                    }
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message, "Playback failed", MessageBoxButton.OK);
                }

            }
                );

            return task1;
        }

        public Task StartPlayAsync4( string path)
        {
            Task task1 = Task.Run(() =>
            {
                try
                {
                    System.IO.DirectoryInfo di = new DirectoryInfo(path);
                    List<FileInfo> playFiles = new List<FileInfo>(di.GetFiles("*.json"));

                    foreach (FileInfo playfile in playFiles)
                    {
                        using (StreamReader fs = playfile.OpenText())
                        {
                            string jsonString = fs.ReadToEnd();
                            JsonMessage jc = JsonConvert.DeserializeObject<JsonMessage>(jsonString);
                            if (jc.EnablePlay)
                            {
                                //jc.Header.ContaingFile = playfile.Name;
                                NowPlayingHeader = jc.Header;
                                for (int i = 0; i < jc.Header.NumberOfCycles; i++)
                                {
                                    TimeSpan ts = TimeSpan.FromMilliseconds(jc.Header.WaitStateMs);
                                    System.Threading.Thread.Sleep(ts);
                                }
                            }
                            // Make EthMessage

                            // play
                        }

                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Playback failed", MessageBoxButton.OK);
                }

            });

            return task1;

            //List<string> playFiles = Directory.GetFiles(path.ToString(), "*,json");

        }

        public void AbortPlay()
        {
            _abortPlayRequest = true;
        }

    }
}
