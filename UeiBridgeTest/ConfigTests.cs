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
    class ConfigTests
    {
        //[Test]
        //public void NoConfigFile()
        //{
        //    Assert.Throws<System.IO.FileNotFoundException>(() =>
        //    {
        //        var c2 = Config2.LoadConfigFromFile(new System.IO.FileInfo("nofile.config"));
        //    });
        //}
        //[Test]
        //public void BadConfigTest()
        //{
        //    Assert.Throws<System.InvalidOperationException>(() =>
        //    {
        //        var c2 = Config2.LoadConfigFromFile(new System.IO.FileInfo("UeiBridgeTest.exe.config"));
        //    });
        //}

        //[Test]
        //public void BuildNewConfigFile()
        //{
        //    System.IO.File.Delete("newfile.config");
        //    string url = "simu://";
        //    CubeSetup cs = new CubeSetup(new List<UeiDeviceInfo>());
        //    Config2 c2 = new Config2(new List<CubeSetup> { cs });
        //    c2.SaveAs(new FileInfo("newfile.config"), true);
        //}
        [Test]
        public void BuildAndSaveConfig3Test()
        {
            File.Delete("Cube2.config");
            File.Delete("Cube.simu.config");

            string simu_url = "simu://";
            string existing_cube = "pdna://192.168.100.2";
            string non_existing_cube = "pdna://192.168.100.99";
            string bad_url = "ksjdlkfjlaks";
            UeiBridge.Library.Config2 c3 = null;// UeiBridge.Library.Config2.LoadConfig(new List<string>() { simu_url, existing_cube, non_existing_cube, bad_url });

            IPAddress ip = UeiBridge.Library.StaticMethods.CubeUrlToIpAddress("pdna://192.168.100.2");
            if (null == CubeSeeker.TryIP(ip))
            {
                Assert.That(c3.CubeSetupList.Count, Is.EqualTo(1));
            }
            else
            {
                Assert.That(c3.CubeSetupList.Count, Is.EqualTo(2));
            }

            //var list1 = new List<UeiDeviceInfo>();
            //var list2 = new List<UeiDeviceInfo>();
            //list1.Add(new UeiDeviceInfo(url1, 0, DeviceMap2.SimuAO16Literal));
            //list2.Add(new UeiDeviceInfo(url2, 0, DeviceMap2.SimuAO16Literal));
            //UeiBridge.LibraryA.CubeSetup cs1 = new UeiBridge.LibraryA.CubeSetup(list1);
            //UeiBridge.LibraryA.CubeSetup cs2 = new UeiBridge.LibraryA.CubeSetup(list2);
            //UeiBridge.LibraryA.Config2 c3 = new UeiBridge.LibraryA.Config2(new List<UeiBridge.LibraryA.CubeSetup> { cs1, cs2 });
            //c3.SavePerCube("UeiBridge", true);
        }
        [Test]
        public void LoadCubeSetupFromFileTest()
        {
            CubeSetupLoader csl = new CubeSetupLoader();
            csl.LoadSetupFile( new FileInfo( "Cube.fortest.config"));
            //CubeSetup cs = CubeSetup.LoadCubeSetupFromFile( new FileInfo( "Cube.fortest.config"));
            Assert.That(csl.CubeSetupMain, Is.Not.Null);
        }
        [Test]
        public void ConfigBadUrlTest()
        {
            UeiBridge.Library.Config2 c2a = null;// UeiBridge.Library.Config2.LoadConfig(new List<string>() { "kkk" });
            Assert.That(c2a.CubeSetupList.Count, Is.EqualTo(0));
        }
        [Test]
        public void LoadConfig3Test()
        {
            //string cubeurl = "pdna://192.168.100.4";
            //string cubeurl = "simu://";
            List<string> urllist = new List<string>() { "simu://", "pdna://192.168.100.15" };
            UeiBridge.Library.Config2 c2a = null;// UeiBridge.Library.Config2.LoadConfig(urllist);

            Assert.That(c2a.CubeSetupList.Count, Is.EqualTo(1));
            Assert.That(c2a.CubeSetupList[0].DeviceSetupList.Count, Is.EqualTo(11));
            //UeiDaq.DeviceCollection devColl = new UeiDaq.DeviceCollection(cubeurl);
            //List<UeiDeviceInfo> devList1 = DeviceCollectionToDeviceInfoList(devColl, );
            //List<UeiDeviceInfo> devList2 = DeviceCollectionToDeviceInfoList(devColl, );

            //List<List<UeiDeviceInfo>> deviceListList = new List<List<UeiDeviceInfo>>();
            //deviceListList.Add(devList1);
            //deviceListList.Add(devList2);

            //Config3 _mainConfig;
            //List<string> cubeUrlList = new List<string>() { "simu://", "192.168.100.4" };
            //try

            //_mainConfig = Config3.LoadConfig( "basefilename");

            //catch (FileNotFoundException)
            //{
            //    var t = Config3.BuildDefaultConfigPerCube(cubeUrlList);
            //    t.SaveAs(new FileInfo(Config2.DafaultSettingsFilename), true);
            //    _mainConfig = Config3.LoadConfigFromFile(new FileInfo("DafaultSettingsFilename"));
            //    Console.WriteLine($"New default settings file created. {Config2.DafaultSettingsFilename}.");
            //}
            //catch (InvalidOperationException ex)
            //{
            //    // ($"Failed to load configuration. {ex.Message}. Any key to abort....");
            //    Console.ReadKey();
            //}
            //catch (Exception ex)
            //{
            //    // ($"Failed to load configuration. {ex.Message}. Any key to abort....");
            //    Console.ReadKey();
            //}

        }

        //private static List<UeiDeviceInfo> DeviceCollectionToDeviceInfoList( DeviceCollection devColl, string cubeurl)
        //{
        //    var l1 = devColl.Cast<UeiDaq.Device>().ToList();
        //    var devList = l1.Select((UeiDaq.Device i) =>
        //    {
        //        if (i == null)
        //            return null;
        //        //Uri url = new Uri(i.GetResourceName());
        //        //string curl = url.LocalPath;
        //        return new UeiDeviceInfo(cubeurl, i.GetIndex(), i.GetDeviceName());
        //    });
        //    return devList.ToList();
        //}

        /// <summary>
        /// Verify that CubeSetup does not generate device-setup for an unknown device.
        /// </summary>
        [Test]
        public void CubeSetupTest1()
        {
            List<UeiDeviceInfo> devList = new List<UeiDeviceInfo>();
            devList.Add(new UeiDeviceInfo("cubeurl", 51, "devicename1"));
            CubeSetup cs = new CubeSetup(devList);
            Assert.That(cs.DeviceSetupList.Count, Is.EqualTo(1));
        }
        /// <summary>
        /// Verify that CubeSetup does generate device-setup for a known device.
        /// </summary>
        [Test]
        public void CubeSetupTest2()
        {
            List<UeiDeviceInfo> devList = new List<UeiDeviceInfo>();
            devList.Add(new UeiDeviceInfo("cubeurl", 101, "AO-308"));
            CubeSetup cs = new CubeSetup(devList);
            Assert.That(cs.DeviceSetupList.Count, Is.EqualTo(1));
            Assert.That(cs.DeviceSetupList[0], Is.Not.Null);
        }

        [Test]
        public void BuildDefaultSimuConfigTest()
        {
            Config2 c2 = new Config2();
            Config2 c3 = null;// Config2.BuildDefaultConfig(new List<string> { "simu://" });
            Assert.That(c3.AppSetup.StatusViewerEP, Is.Not.Null);
            Assert.That(c3.CubeSetupList[0].DeviceSetupList.Count, Is.EqualTo(11)); // only one simulation device setup is defined.
        }

        //[Test]
        //public void CompareConfigTest()
        //{
        //    string filename = "setupForTest.config";

        //    Config2 c1 = Config2.BuildDefaultConfig(new List<string> { "simu://" });
        //    c1.SaveAs(new FileInfo(filename), true);

        //    Config2 c2 = Config2.LoadConfigFromFile(new FileInfo(filename));
        //    Config2 c3 = Config2.LoadConfigFromFile(new FileInfo(filename));
        //    c2.CubeSetupList.FirstOrDefault().DeviceSetupList.FirstOrDefault().DeviceName = "kkk";
        //    Assert.That(c2.Equals(c3), Is.False);
        //}

        [Test]
        public void LoadConnectedCubesSetup_NoFile()
        {
            string fn = CubeSetup.GetSelfFilename(UeiDeviceInfo.SimuCubeId);
            if (File.Exists(fn))
            {
                File.Delete(fn);
            }
            List<string> urlList = new List<string>() { "simu://" };
            List<CubeSetup> list = Config2.GetSetupForCubes(urlList);
            CubeSetupLoader.SaveSetupFile(list[0], new FileInfo(fn));

            Assert.Multiple(() =>
            {
                Assert.That(File.Exists(fn), Is.EqualTo(true));
                Assert.That(list[0].DeviceSetupList.Count, Is.EqualTo(11));
                //Assert.That(list[0].AssociatedFileFullname, Is.Not.Null);
            });
        }
        [Test]
        public void LoadConnectedCubesSetup_badFile()
        {
            List<string> urlList = new List<string>() { "simu://" };
            FileInfo fi = new FileInfo("Cube.simu.config");
            using (var t = fi.CreateText())
            {
                t.WriteLine("bad-xml");
            }

            Assert.Throws<System.InvalidOperationException>(() =>
            {
                List<CubeSetup> list = Config2.GetSetupForCubes(urlList);
            });
        }
        [Test]
        public void LoadConnectedCubesSetup_badUrl()
        {
            List<string> urlList = new List<string>() { "aaa" };
            List<CubeSetup> list = Config2.GetSetupForCubes(urlList);
            Assert.That(list.Count, Is.EqualTo(0));
        }
        private static void CreateBackupFile(FileInfo fi)
        {
            string barename = Path.GetFileNameWithoutExtension(fi.Name);
            string ext = Path.GetExtension(fi.Name);
            int index = 1;
            while (true)
            {
                string backname = $"{barename} ({index}){ext}";
                if (File.Exists(backname))
                {
                    index++;
                    continue;
                }
                else
                {
                    fi.CopyTo(backname);
                    break;
                }
            }
        }
    }

}
