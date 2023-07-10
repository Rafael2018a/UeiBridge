using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using UeiDaq;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace ScanNetwork
{
   class Program
   {
      static void Main(string[] args)
      {
         String network = "192.168.100.";
         int numAddresses = 250;
         Thread[] scanThreads = new Thread[numAddresses+1];

         for (int n = 1; n <= numAddresses; n++)
         {
            String ipAddress = network + n.ToString();
            scanThreads[n] = new Thread(new ParameterizedThreadStart(Program.TryIP));
            scanThreads[n].Start(ipAddress);
         }

         //Thread.Sleep(1000);

         // Wait for all threads to complete
         for (int n = 1; n <= numAddresses; n++)
         {
            scanThreads[n].Join();
         }
      }

      public static void TryIP(Object obj)
      {
         String ipAddress = (String)obj;

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

            

            IPEndPoint ep = new IPEndPoint(IPAddress.Parse(ipAddress), 6334);
            udpClient.Send(sendBuffer, (int)writer.BaseStream.Length, ep);

            udpClient.Client.ReceiveTimeout = 500;
            Byte[] recvBuffer = udpClient.Receive(ref ep);
            BinaryReader reader = new BinaryReader(new MemoryStream(recvBuffer));
            uint prolog = (uint)IPAddress.NetworkToHostOrder(unchecked((int)reader.ReadUInt32()));
            ushort ts = (ushort)IPAddress.NetworkToHostOrder(unchecked((short)reader.ReadUInt16()));
            ushort cnt = (ushort)IPAddress.NetworkToHostOrder(unchecked((short)reader.ReadUInt16()));
            uint cmd = (uint)IPAddress.NetworkToHostOrder(unchecked((int)reader.ReadUInt32()));
            uint reqid = (uint)IPAddress.NetworkToHostOrder(unchecked((int)reader.ReadUInt32()));

            if(cmd == 0x1104)
            {
               Console.WriteLine("Found IOM at " + ipAddress);
            }
         }
         catch(SocketException ex)
         {
            //Console.WriteLine(ex.ToString());
         }
         udpClient.Close();
      }
   }
}
