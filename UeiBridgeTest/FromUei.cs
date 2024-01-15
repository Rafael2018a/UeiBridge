using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UeiBridge;
using NUnit.Framework;
using System.IO;
using UeiBridge.Library;
using UeiDaq;
using System.Net;
using UeiBridge.CubeSetupTypes;

namespace UeiBridgeTest
{
    [TestFixture]
    class FromUei
    {
        [Test]
        public void ClearACBRunAwaySession_test()
        {
            DasReset();
     
        }

        static void DasReset()
        {

            string sDevCollName = "";
            Device myDevice;
            string pdnaIpAddress = "pdna://192.168.100.3";
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
                    int nModelNum = Convert.ToInt16(sModel);
                    if (nModelNum > 100)
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


    }
}
