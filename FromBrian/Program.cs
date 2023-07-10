using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UeiDaq;

namespace TestOpenCloseSession
{
    class Program
    {

        static void DigitalInSessionTest()
        {
            int loopCount = 10000;
            for (int i = 0; i < loopCount; i++)
            {
                try
                {
                    Session s1 = new Session();
                    s1.CreateDIChannel("pdna://192.168.100.50/Dev3/Di0:1"); // this line might throw

                    Range[] rg = s1.GetDevice().GetDIRanges();
                    s1.ConfigureTimingForSimpleIO();

                    s1.Start();

                    var _reader = new DigitalReader(s1.GetDataStream());
                    var val = _reader.ReadSingleScanUInt32();

                    s1.Stop();

                    s1.Dispose();
                    System.Threading.Thread.Sleep(400);
                    System.Console.WriteLine("Start/Stop loop count: {0}", i);
                }
                catch (UeiDaq.UeiDaqException ex)
                {
                    System.Console.WriteLine("Error " + ex);
                    Environment.Exit(0);
                }
            }
        }

        static void Main(string[] args)
        {
            DigitalInSessionTest();
        }
    }
}
