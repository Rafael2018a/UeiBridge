using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using UeiBridge.Library.Types;
using UeiBridge.Library.Interfaces;
using UeiBridge.Library;

namespace UeiBridge
{
#if old
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
            if (IPAddress.TryParse( Config.Instance.SenderMulticastAddress, out ip))
            {
                int p = Config.Instance.SenderMulticastPort;
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

            //_logger.Debug($"Message Sent through udp....len={message.Length}");
            
        }
    }
#endif
    public class UdpWriterAsync : IEnqueue<SendObject2>, IDisposable
    {
        log4net.ILog _logger = StaticLocalMethods.GetLogger();
        Socket _sendSocket;
        UdpClient _udpClient = new UdpClient();
        bool _inDispose = false;

        public UdpWriterAsync(IPEndPoint destEp)
        {
            _udpClient.Connect(destEp);
        }
        public void Dispose()
        {
            _udpClient.Dispose();
        }

        public void Enqueue(SendObject2 sendObj)
        {
            if (_inDispose==false)
            {
                byte[] buf = sendObj.MessageBuild(sendObj.RawByteMessage);
                _udpClient.Send(buf, buf.Length);
            }
        }
    }
    public class UdpWriter : ISend<SendObject>, IDisposable
    {
        log4net.ILog _logger = StaticLocalMethods.GetLogger();
        Socket _sendSocket;
        //string _instanceName;
        public UdpWriter( IPEndPoint destEp, string localBindAddress)
        {

            //this._instanceName = instnceName;
            System.Diagnostics.Debug.Assert(null != localBindAddress);
            try
            {
                // Create socket
                _sendSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                // Multicast IP-address
                //IPAddress _mcastDestAddress = IPAddress.Parse(destAddress); //Config.Instance.DestMulticastAddress);

                // Join multicast group
                //_sendSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(_mcastDestAddress));

                // TTL
                _sendSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, 2);

                // Create an endpoint
                //IPEndPoint _mcastDestEP = new IPEndPoint(_mcastDestAddress, destPort);// Config.Instance.DestMulticastPort);

                string usingNic=null;
                if (null != localBindAddress)
                {
                    IPEndPoint localEP = new IPEndPoint(IPAddress.Parse(localBindAddress), 0);
                    try
                    {
                        _sendSocket.Bind(localEP);
                        usingNic = $"Using NIC: {localBindAddress}";
                    }
                    catch(Exception ex)
                    {
                        _logger.Warn($" Failed to bind to local NIC {localBindAddress}. {ex.Message}");
                    }
                }
                else
                {
                    usingNic = "(no specific NIC)";
                }
                // Connect to the endpoint
                //_sendSocket.Connect(_mcastDestEP);
                

                //_logger.Info($"Multicast sender - {this._instanceName} - esablished. Dest:{destEp.ToString()}. {usingNic}");
            }
            catch (SocketException ex)
            {
                _logger.Warn(ex.Message);
            }
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

        //public void Send(byte[] buffer)
        //{
        //    if (_sendSocket.Connected)
        //    {
        //        _sendSocket.Send(buffer, buffer.Length, SocketFlags.None);
        //    }
        //}

        public void Dispose()
        {
            _sendSocket.Close();
        }

        public void Send(SendObject sendObj)
        {
            _sendSocket.SendTo(sendObj.ByteMessage, SocketFlags.None, sendObj.TargetEndPoint);
        }
    }
}
