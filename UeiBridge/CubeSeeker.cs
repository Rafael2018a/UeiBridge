using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace UeiBridge
{
    class CubeSeeker
    {
        IPAddress SearchCube_TM( object state)
        {
            IPAddress ip = (IPAddress)state;
            System.Diagnostics.Debug.Assert(null != ip);
            var ipref = IPAddress.Parse("192.168.100.44");
            if (ip.ToString() == ipref.ToString())
                return ip;
            else
                return null;
        }
        public static List<IPAddress> FindCubesInRange(IPAddress startAddress, uint range)
        {
            byte[] addressBytes = startAddress.GetAddressBytes().ToArray();
            uint b3first = addressBytes[3];
            uint b3last = b3first + range;
            System.Diagnostics.Debug.Assert(b3last < 254);

            //var tasks = new Task<IPAddress>[range];
            List<Task<IPAddress>> taskList = new List<Task<IPAddress>>();

            //uint b3 = 1;
            for (uint b3 = b3first; b3<b3last; b3++)
            {
                byte[] ab = addressBytes;
                ab[3] = (byte)b3;

                taskList.Add( Task.Factory.StartNew(new Func<object, IPAddress>(TryIP), new IPAddress(ab), TaskCreationOptions.LongRunning));
            }

            var x = taskList.Where(t => t.Result != null).Select( t=> t.Result);

            return x.ToList<IPAddress>();

        }
        void f()
        {
            IPAddress ip = new IPAddress(new byte[] { 10, 20, 30, 40 });
            //ip++;

        }
        public static IPAddress TryIP(Object obj)
        {
            IPAddress ipAddress = (IPAddress)obj;
            //Console.WriteLine($"checking {ipAddress}");

            UdpClient udpClient = new UdpClient();
            try
            {
                Byte[] sendBuffer = new byte[255];
                BinaryWriter writer = new BinaryWriter(new MemoryStream(sendBuffer));

                writer.Write((uint)IPAddress.HostToNetworkOrder(unchecked((int)0xbabafaca)));
                writer.Write((ushort)IPAddress.HostToNetworkOrder(0));
                writer.Write((ushort)IPAddress.HostToNetworkOrder(0));
                writer.Write((uint)IPAddress.HostToNetworkOrder(unchecked((int)0x104)));
                writer.Write((uint)IPAddress.HostToNetworkOrder(0));



                IPEndPoint ep = new IPEndPoint(ipAddress, 6334);
                udpClient.Send(sendBuffer, (int)writer.BaseStream.Length, ep);

                udpClient.Client.ReceiveTimeout = 500;
                Byte[] recvBuffer = udpClient.Receive(ref ep);
                BinaryReader reader = new BinaryReader(new MemoryStream(recvBuffer));
                uint prolog = (uint)IPAddress.NetworkToHostOrder(unchecked((int)reader.ReadUInt32()));
                ushort ts = (ushort)IPAddress.NetworkToHostOrder(unchecked((short)reader.ReadUInt16()));
                ushort cnt = (ushort)IPAddress.NetworkToHostOrder(unchecked((short)reader.ReadUInt16()));
                uint cmd = (uint)IPAddress.NetworkToHostOrder(unchecked((int)reader.ReadUInt32()));
                uint reqid = (uint)IPAddress.NetworkToHostOrder(unchecked((int)reader.ReadUInt32()));

                if (cmd == 0x1104)
                {
                    return ipAddress;
                }
                else
                    return null;
            }
            catch (SocketException ex)
            {
                //Console.WriteLine(ex.ToString());
                return null;
            }
            //udpClient.Close();
        }

    }
}
