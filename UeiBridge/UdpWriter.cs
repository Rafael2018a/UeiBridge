﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace UeiBridge
{
    class UdpWriter_old : ISend<byte[]>
    {
        log4net.ILog _logger = log4net.LogManager.GetLogger("Root");
        UdpClient _udpClient;
        public UdpWriter_old()
        {
            IPAddress local = IPAddress.Parse("221.109.251.103");
            IPEndPoint localep = new IPEndPoint(local, 5050);
            _udpClient = new UdpClient();
            IPAddress ip;
            if (IPAddress.TryParse( Config.Instance.DestMulticastAddress, out ip))
            {
                int p = Config.Instance.DestMulticastPort;
                _udpClient.Connect( ip, p);
                _logger.Info($"Multicast sender esablished. Target end point {ip}:{p}");
            }
        }
        public void Send(byte[] message)
        {
            if (null == message)
            {
                _logger.Warn("Can't send null message");
                return;
            }
            _udpClient.Send(message, message.Length);

            _logger.Debug($"Message Sent through udp....len={message.Length}");
        }
    }

    class UdpWriter : ISend<byte[]>, IDisposable
    {

        Socket _sendSocket;
        public UdpWriter()
        {

            // Create socket
            _sendSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            // Multicast IP-address
            IPAddress _mcastDestAddress = IPAddress.Parse(Config.Instance.DestMulticastAddress);

            // Join multicast group
            _sendSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(_mcastDestAddress));

            // TTL
            _sendSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, 2);

            // Create an endpoint
            IPEndPoint _mcastDestEP = new IPEndPoint(_mcastDestAddress, Config.Instance.DestMulticastPort);

            IPEndPoint localEP = new IPEndPoint(IPAddress.Parse( Config.Instance.LocalBindNicAddress), 11011);
            _sendSocket.Bind(localEP);
            // Connect to the endpoint
            _sendSocket.Connect(_mcastDestEP);

            // Scan message
            //while (true)
            //{
            //    byte[] buffer = new byte[1024];
            //    string msg = Console.ReadLine();
            //    buffer = Encoding.ASCII.GetBytes(msg);
            //    _sendSocket.Send(buffer, buffer.Length, SocketFlags.None);
            //    if (msg.Equals("Bye!", StringComparison.Ordinal))
            //        break;
            //}

        }

        public void Send(byte[] buffer)
        {
            _sendSocket.Send(buffer, buffer.Length, SocketFlags.None);
        }

        public void Dispose()
        {
            _sendSocket.Close();
        }
    }
}
