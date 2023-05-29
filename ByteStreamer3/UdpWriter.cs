using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace ByteStreamer3
{
    public class UdpWriter 
    {
        Socket _sendSocket;
        //string _instanceName;
        public UdpWriter( IPEndPoint destEp, IPAddress localBindAddress = null )
        {
            try
            {
                // Create socket
                _sendSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                // TTL
                _sendSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, 2);

                if (null != localBindAddress)
                {
                    IPEndPoint localEP = new IPEndPoint( localBindAddress, 0);
                    try
                    {
                        _sendSocket.Bind(localEP);
                    }
                    catch(Exception )
                    {
                        System.Diagnostics.Debug.Assert(false);
                    }
                }
                // Connect to the endpoint
                _sendSocket.Connect( destEp);
                
            }
            catch (SocketException )
            {
                System.Diagnostics.Debug.Assert(false);
            }
        }

        public void Dispose()
        {
            _sendSocket.Close();
        }

        public void Send( byte [] byteMessage)
        {
            _sendSocket.Send(byteMessage);//, SocketFlags.None, sendObj.TargetEndPoint);
        }
    }
}
