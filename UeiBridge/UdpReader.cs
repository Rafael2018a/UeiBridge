#define mcast

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UeiBridge.Types;

namespace UeiBridge
{
    /// <summary>
    /// Reads datagrams from udp channel and sends them to 'consumer'.
    /// </summary>
    public class UdpReader: IDisposable
    {
        private IEnqueue<byte[]> _datagramConsumer;
        log4net.ILog _logger = StaticMethods.GetLogger();
        UdpClient _udpclient;
        string _instanceName;
        IPEndPoint _msListeningiEp;
        IPAddress _localNIC;

        public UdpReader( IPEndPoint listeninigEp, IPAddress localNIC, IEnqueue<byte[]> consumer, string instanceName)
        {
            this._datagramConsumer = consumer;
            this._instanceName = instanceName;
            this._msListeningiEp = listeninigEp;
            this._localNIC = (localNIC==null)? IPAddress.Any : localNIC;
            System.Diagnostics.Debug.Assert(instanceName.Length > 1);
        }

        //internal void Start()
        //{
        //    //IPAddress mcastAddress;
        //    //if (IPAddress.TryParse(Config.Instance.ReceiverMulticastAddress, out mcastAddress))
        //    EstablishMulticastReceiver();
        //}
        
        public void Start()
        {
            //int _port;
            //IPAddress _multicastIPaddress;

            // Store params
            //_multicastIPaddress = multicastAddress;
            //_port = port;

            try
            {

                //IPAddress localIP = (localIPaddress == null) ? IPAddress.Any : localIPaddress;
                //IPAddress localIP = IPAddress.Any;

                // Create endpoints
                //IPEndPoint _remoteEndPoint = new IPEndPoint(multicastAddress, multicastPort);

                // Create and configure UdpClient
                _udpclient = new UdpClient();

                // The following three lines allow multiple clients on the same PC
                _udpclient.ExclusiveAddressUse = false;
                _udpclient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                _udpclient.ExclusiveAddressUse = false;

                // Bind
                IPEndPoint _localEndPoint = new IPEndPoint(_localNIC, _msListeningiEp.Port);
                _udpclient.Client.Bind(_localEndPoint);

                // join
                _udpclient.JoinMulticastGroup(_msListeningiEp.Address, _localNIC);

                //_logger.Info($"Multicast receiver - {this._instanceName} - esablished. Listening on {_msListeningiEp}");
                // Start listening for incoming data
                _udpclient.BeginReceive(new AsyncCallback(ReceivedCallback), null);
            }
            catch (SocketException ex)
            {
                _logger.Warn( $"{_instanceName}: Faild to establish multicast receiver. Group {_msListeningiEp}. {ex.Message}");
            }
        }

        /// <summary>
        /// Callback which is called when UDP packet is received
        /// </summary>
        private void ReceivedCallback(IAsyncResult ar)
        {
            // Get received data
            IPEndPoint sender = new IPEndPoint(0, 0);

            try
            {
                Byte[] receivedBytes = _udpclient.EndReceive(ar, ref sender);
                this._datagramConsumer.Enqueue(receivedBytes);
                _udpclient.BeginReceive(new AsyncCallback(ReceivedCallback), null);// Restart listening 
            }
            catch (ObjectDisposedException)
            {
                // nothing to do here
            }
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

        public void Dispose()
        {
            _udpclient.Dispose();
        }
    }


}