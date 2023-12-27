using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UeiDaq;

namespace serial
{
    class Program
    {
        static string resourceStr = "pdna://192.168.100.2/dev1/com4:7";
        static Session srSession;
        static SerialWriter[] srWriter;
        static SerialReader[] srReader;
        static SerialPort[] myPort;
        static void Main(string[] args)
        {
            Program p = new Program();

            p.Run();
        }

        private void Run()
        {
            int ch;
            srSession = new Session();

            srSession.CreateSerialPort(resourceStr,
                                        SerialPortMode.RS485FullDuplex,
                                        SerialPortSpeed.BitsPerSecond57600,
                                        SerialPortDataBits.DataBits8,
                                        SerialPortParity.None,
                                        SerialPortStopBits.StopBits1,
                                        "");

            srSession.ConfigureTimingForMessagingIO(100, 10.0);
            srSession.GetTiming().SetTimeout(5);

            srWriter = new SerialWriter[srSession.GetNumberOfChannels()];
            srReader = new SerialReader[srSession.GetNumberOfChannels()];
            myPort = new SerialPort[srSession.GetNumberOfChannels()];

            int numChan = srSession.GetNumberOfChannels();
            for (int chIndex = 0; chIndex < numChan; chIndex++)
            {
                ch = srSession.GetChannel(chIndex).GetIndex();
                myPort[chIndex] = (SerialPort)srSession.GetChannel(chIndex);
                if (ch == 4)
                {
                    //myPort[chIndex].SetCustomSpeed(115200);
                    //myPort[chIndex].SetSpeed(SerialPortSpeed.BitsPerSecond115200); commented by Rafi
                    myPort[chIndex].SetSpeed(SerialPortSpeed.BitsPerSecond14400); // this will cause exception at line 75
                }
                else if (ch == 5)
                {
                    //myPort[chIndex].SetCustomSpeed(38400);
                    myPort[chIndex].SetSpeed(SerialPortSpeed.BitsPerSecond38400);
                    
                }
                else if (ch == 6)
                {
                    //myPort[chIndex].SetCustomSpeed(128000);
                    myPort[chIndex].SetSpeed(SerialPortSpeed.BitsPerSecond128000);
                }
                else if (ch == 7)
                {
                    //myPort[chIndex].SetCustomSpeed(250000);
                    myPort[chIndex].SetSpeed(SerialPortSpeed.BitsPerSecond250000);
                }
                srWriter[chIndex] = new SerialWriter(srSession.GetDataStream(), ch);
                srReader[chIndex] = new SerialReader(srSession.GetDataStream(), ch);
            }

            srSession.Start();
            // 100 bytes every 10 ms

            byte[] binMsg = new byte[100];
            for (int k = 0; k < 100; k++)
            {
                binMsg[k] = (byte)k;
            }

            int line = 0;
            int totallen = 0;
            int len = 0;
            for (int j = 0; j < 50; j++)
            {
                for (int chIndex = 0; chIndex < numChan; chIndex++)
                {
                    try
                    {
                        len = srWriter[chIndex].Write(binMsg);

                        System.Diagnostics.Debug.Assert(len == binMsg.Length);
                        totallen += len;

                        byte[] rxMsg = srReader[chIndex].Read(10);

                    }
                    catch (UeiDaqException ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    System.Threading.Thread.Sleep(10);
                    Console.WriteLine($"{line++}> {totallen}");
                }
            }

            srSession.Stop();
            srSession.GetDevice().Reset();
            srSession.Dispose();

        }
    }
}
