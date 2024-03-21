using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UeiBridge.Library;

namespace StatusViewerTester
{
    class Program
    {
        static void Main(string[] args)
        {
            Program p = new Program();
            p.Run();
        }

        private void Run()
        {
            PublishStatus_Task();
        }


        void PublishStatus_Task()
        {
            //const int intervalMs = 100;
            IPEndPoint destEP = new IPEndPoint(IPAddress.Parse("239.10.10.17"), 5093);
            UdpClient udpClient = new UdpClient();
            udpClient.Connect(destEP);

            TimeSpan interval = TimeSpan.FromMilliseconds(1000);

            int count = 0;
            // get formatted string for each device in list
            while (true)
            {
                ++count;

                byte[] send_buffer = BuildMessage(StatusTrait.IsRegular, "msg1", count);
                udpClient.Send(send_buffer, send_buffer.Length);
                send_buffer = BuildMessage(StatusTrait.IsWarning, "msg2", count);
                udpClient.Send(send_buffer, send_buffer.Length);

                System.Threading.Thread.Sleep(interval);
            }
        }

        private static byte[] BuildMessage(StatusTrait st, string desc1, int count)
        {
            string desc = desc1;
            StatusTrait tr = st;
            string[] stat = { $"AAA {count}", $"BBB {count}" };
            StatusEntryJson js = new StatusEntryJson(desc, stat, tr);
            string s = Newtonsoft.Json.JsonConvert.SerializeObject(js);
            byte[] send_buffer = Encoding.ASCII.GetBytes(s);
            return send_buffer;
        }
    }
}
