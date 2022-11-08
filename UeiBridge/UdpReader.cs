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
        string _instanceName;

        public UdpReader(IEnqueue<byte[]> consumer, string name)
        {
            this._datagramConsumer = consumer;
            this._instanceName = name;
        }

        internal void Start()
        {
            IPAddress mcastAddress;
            if (IPAddress.TryParse(Config.Instance.ReceiverMulticastAddress, out mcastAddress))
            {
                EstablishMulticastReceiver(mcastAddress, Config.Instance.ReceiverMulticastPort, IPAddress.Parse(Config.Instance.LocalBindNicAddress));
            }
            else
            {
                _logger.Warn($"Fail to parse ip {Config.Instance.ReceiverMulticastAddress}. Udp Receiver disabled.");
            }
        }
        
        public void EstablishMulticastReceiver(IPAddress multicastAddress, int multicastPort, IPAddress localIPaddress = null)
        {
            //int _port;
            //IPAddress _multicastIPaddress;

            // Store params
            //_multicastIPaddress = multicastAddress;
            //_port = port;

            try
            {

                IPAddress localIP = (localIPaddress == null) ? IPAddress.Any : localIPaddress;

                // Create endpoints
                IPEndPoint _remoteEndPoint = new IPEndPoint(multicastAddress, multicastPort);

                // Create and configure UdpClient
                _udpclient = new UdpClient();

                // The following three lines allow multiple clients on the same PC
                _udpclient.ExclusiveAddressUse = false;
                _udpclient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                _udpclient.ExclusiveAddressUse = false;

                // Bind
                IPEndPoint _localEndPoint = new IPEndPoint(localIP, multicastPort);
                _udpclient.Client.Bind(_localEndPoint);

                // join
                _udpclient.JoinMulticastGroup(multicastAddress, localIP);

                _logger.Info($"Multicast receiver - {this._instanceName} - esablished. Listening on {_remoteEndPoint}");
                // Start listening for incoming data
                _udpclient.BeginReceive(new AsyncCallback(ReceivedCallback), null);
            }
            catch (SocketException ex)
            {
                _logger.Warn( $"Faild to establish multicast receiver from group {multicastAddress}. {ex.Message}");
            }
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

            
            _udpclient.BeginReceive(new AsyncCallback(ReceivedCallback), null);// Restart listening 
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