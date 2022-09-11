using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UeiDaq;

namespace SerialWriter
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
            var serialSession = new Session();
            string termStr = @"\n";
            termStr = termStr.Replace(@"\r", "\r");
            termStr = termStr.Replace(@"\n", "\n");

            serialSession.CreateSerialPort("pdna://192.168.100.2/Dev3/Com0,1",
                                        SerialPortMode.RS232,
                                        SerialPortSpeed.BitsPerSecond9600,
                                        SerialPortDataBits.DataBits8,
                                        SerialPortParity.None,
                                        SerialPortStopBits.StopBits1,
                                        termStr);

            serialSession.ConfigureTimingForMessagingIO(100, 100.0);
            //serialSession.GetTiming().SetTimeout(500);

            var i = serialSession.GetChannel(1).GetIndex();
            var serialWriter = new UeiDaq.SerialWriter(serialSession.GetDataStream(), i);

            int line = 0;
            for (int j=0; j<10; j++)
            {
                string msg = "hello iai";
                //msg = msg.Replace(@"\r", "\r");
                //msg = msg.Replace(@"\n", "\n");

                serialWriter.Write(System.Text.Encoding.ASCII.GetBytes(msg));
                System.Threading.Thread.Sleep(500);

                Console.WriteLine($"{line++}> " + msg);
            }

            serialSession.Stop();
        }
    }
}
