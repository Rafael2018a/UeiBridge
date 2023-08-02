using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using UeiDaq;

namespace UeiBridgeTest
{
    [TestFixture]
    internal class FromBrian
    {
        [Test]
        public void deviceReset()
        {
            Program1 p1 = new Program1();
            Program1.DasReset();
        }
        [Test]
        public void resetTest()
        {
            var myDevice = DeviceEnumerator.GetDeviceFromResource("simu://Dev2/");
            myDevice.Reset();
        }
    }

    class Program1
    {
        static string[] resourceStr = { "pdna://192.168.100.40/dev0" };

        static int devNum = resourceStr.Length;
        static Session[] mySs = new Session[devNum];
        static AnalogScaledReader[] reader = new AnalogScaledReader[devNum];



        public static void DasReset()
        {

            string sDevCollName = "";
            Device myDevice;
            //string pdnaIpAddress = "pdna://192.168.100.50";
            string pdnaIpAddress = "simu://";
            int nDeviceNumber = 0;
            DeviceCollection devColl = new DeviceCollection(pdnaIpAddress);

            DateTime localdatestartloop = System.DateTime.Now;
            System.Console.WriteLine("Before Loop Date/Time: {0}", localdatestartloop.ToString("yyyy-MM-dd HH:mm:ss:FFF"));

            foreach (Device dev in devColl)
            {
                if (dev != null)
                {
                    sDevCollName = dev.GetDeviceName();
                    int myDevNumber = dev.GetIndex();
                    Console.WriteLine(sDevCollName);

                    // 2020-04-23 JED - added per Brian Dao from UEI to only reset certain layers
                    string[] sDevCollNameSplit = sDevCollName.Split('-');
                    string sModel = sDevCollNameSplit[1];
                    //int nModelNum = Convert.ToInt16(sModel);
                    //if (nModelNum > 100)
                    {
                        string sName = pdnaIpAddress + "/dev" + nDeviceNumber.ToString().Trim() + "/";
                        myDevice = DeviceEnumerator.GetDeviceFromResource(sName);
                        if (myDevice != null)
                        {
                            Console.WriteLine("Reset");
                            myDevice.Reset();
                        }
                    }
                }
                nDeviceNumber++;
            }
            DateTime localdatestoploop = System.DateTime.Now;
            System.Console.WriteLine("After Loop Date/Time: {0}", localdatestoploop.ToString("yyyy-MM-dd HH:mm:ss:FFF"));


        }


        static void Main1(string[] args)
        {


            DasReset();

        }
    }

}
