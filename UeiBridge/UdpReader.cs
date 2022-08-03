#define mcast

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;


namespace UeiBridge
{
    /// <summary>
    /// Reads datagrams from udp channel and sends them to 'consumer'.
    /// </summary>
    internal class UdpReader
    {
        private IEnqueue<byte[]> _datagramConsumer;
        log4net.ILog _logger = StaticMethods.GetLogger();
        UdpClient _udpclient;

        public UdpReader(IEnqueue<byte[]> consumer)
        {
            this._datagramConsumer = consumer;
        }

        internal void Start()
        {
            IPAddress ip;
            if (IPAddress.TryParse(Config.Instance.ReceiverMulticastAddress, out ip))
            {
                //EstablishMulticastReceiver(ip, Config.Instance.LocalPort);
                EstablishMulticastReceiver(ip, Config.Instance.LocalPort, IPAddress.Parse(Config.Instance.LocalBindNicAddress)); // tbd. put ip in config
                
            }
            else
            {
                _logger.Warn($"Fail to parse ip {Config.Instance.ReceiverMulticastAddress}. Udp Receiver disabled.");
            }
        }
        
        public void EstablishMulticastReceiver(IPAddress multicastIPaddress, int port, IPAddress localIPaddress = null)
        {
            IPAddress localIP;
            int _port;
            IPAddress _multicastIPaddress;
            IPEndPoint _localEndPoint;
            IPEndPoint _remoteEndPoint;

            // Store params
            _multicastIPaddress = multicastIPaddress;
            _port = port;
            localIP = localIPaddress;
            if (localIPaddress == null)
                localIP = IPAddress.Any;

            // Create endpoints
            _remoteEndPoint = new IPEndPoint(_multicastIPaddress, port);
            
            // Create and configure UdpClient
            _udpclient = new UdpClient();

            // The following three lines allow multiple clients on the same PC
            _udpclient.ExclusiveAddressUse = false;
            _udpclient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _udpclient.ExclusiveAddressUse = false;

            // Bind, Join
            _localEndPoint = new IPEndPoint(localIP, port);
            _udpclient.Client.Bind(_localEndPoint);

            // join
            _udpclient.JoinMulticastGroup(_multicastIPaddress, localIP);

            _logger.Info($"Multicast receiver esablished. Listening on {_remoteEndPoint}");
            // Start listening for incoming data
            _udpclient.BeginReceive(new AsyncCallback(ReceivedCallback), null);
        }

        /// <summary>
        /// Callback which is called when UDP packet is received
        /// </summary>
        private void ReceivedCallback(IAsyncResult ar)
        {
            // Get received data
            IPEndPoint sender = new IPEndPoint(0, 0);
            Byte[] receivedBytes = _udpclient.EndReceive(ar, ref sender);
            //_logger.Debug($"Datagram received from {sender.Address}, Length={receivedBytes.Length}");

            this._datagramConsumer.Enqueue(receivedBytes);

            // Restart listening for udp data packages
            _udpclient.BeginReceive(new AsyncCallback(ReceivedCallback), null);
        }

        private void SinWave()
        {
            double rawval = 0;
            double samplesPerCycle = 100.0;
            double delta = 2 * Math.PI / samplesPerCycle;

            byte[] byteMessage = new byte[16 + 8];
            byteMessage[5] = 0;

            while (rawval < 1000 * 2 * Math.PI)
            {
                // get message from udp
                double d = 10.0 * Math.Sin(rawval);
                byte[] eight = BitConverter.GetBytes(d);
                eight.CopyTo(byteMessage, 16);

                // send to consumer
                _datagramConsumer.Enqueue(byteMessage);

                rawval += delta;

                Thread.Sleep(1);
            }
        }


    }


}