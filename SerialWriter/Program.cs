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

            serialSession.CreateSerialPort("pdna://192.168.100.2/Dev3/Com0",
                                        SerialPortMode.RS232,
                                        SerialPortSpeed.BitsPerSecond9600,
                                        SerialPortDataBits.DataBits8,
                                        SerialPortParity.None,
                                        SerialPortStopBits.StopBits1,
                                        "");

            //serialSession.ConfigureTimingForMessagingIO(1000, 100.0);
            //serialSession.GetTiming().SetTimeout(500);
            serialSession.ConfigureTimingForSimpleIO();

            var i = serialSession.GetChannel(0).GetIndex();
            var serialWriter = new UeiDaq.SerialWriter(serialSession.GetDataStream(), i);

            // 100 bytes every 10 ms

            byte[] binMsg = new byte[100];
            for(int k=0; k<100; k++)
            {
                binMsg[k] = (byte)k;
            }

            int line = 0;
            int totallen = 0;
            for (int j=0; j<10000; j++)
            {
                try
                {
                    int len = serialWriter.Write(binMsg);
                    System.Diagnostics.Debug.Assert(len == binMsg.Length);
                    totallen += len;
                }
                catch (UeiDaqException ex)
                {
                    Console.WriteLine(ex.Message);
                }
                System.Threading.Thread.Sleep(10);

                Console.WriteLine($"{line++}> {totallen}");
            }

            serialSession.Stop();
        }
    }
}
