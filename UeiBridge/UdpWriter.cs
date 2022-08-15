using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace UeiBridge
{
    class UdpWriter : ISend<byte[]>
    {
        log4net.ILog _logger = log4net.LogManager.GetLogger("Root");
        UdpClient _udpClient;
        public UdpWriter()
        {
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
}
