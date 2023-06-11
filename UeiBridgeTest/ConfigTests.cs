﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UeiBridge;
using NUnit.Framework;
using System.IO;
using UeiBridge.Library;

namespace UeiBridgeTest
{
    [TestFixture]
    class ConfigTests
    {
        //[Test]
        public void GetName()
        {
            var EntAsm = System.Reflection.Assembly.GetEntryAssembly();
            var s = EntAsm.GetName().Name;
            int cube = 3;
            string filename = $"{s}.Cube{cube}.config";

        }
        //[Test]
        //public void LoadConfigTest()
        //{
        //    if (Config2.IsConfigFileExist())
        //        System.IO.File.Delete(Config2.SettingsFilename);

        //    if (!Config2.IsConfigFileExist())
        //    {
        //        Config2.Instance.BuildNewConfig(new string[] { "simu://" });
        //    }
        //    DeviceSetup ds = Config2.Instance.GetSetupEntryForDevice("simu://", 2);

        //    Assert.That(ds, Is.Not.Null);
        //    Assert.That(ds.SlotNumber, Is.EqualTo(2));
        //}

        [Test]
        public void NoConfigFile()
        {
            Assert.Throws<System.IO.FileNotFoundException>(() => 
            {
                var c2 = Config2.LoadConfigFromFile(new System.IO.FileInfo("nofile.config"));
            });
        }
        [Test]
        public void BadConfigTest()
        {
            Assert.Throws<System.InvalidOperationException>(() => 
            { 
                var c2 = Config2.LoadConfigFromFile(new System.IO.FileInfo("UeiBridgeTest.exe.config"));
            });
        }

        [Test]
        public void BuildNewConfigFile()
        {
            System.IO.File.Delete("newfile.config");
            string url = "simu://";
            CubeSetup cs = new CubeSetup(new List<UeiDeviceInfo>(), url);
            Config2 c2 = new Config2(new List<CubeSetup> { cs });
            c2.SaveAs(new FileInfo("newfile.config"), true);
        }
        [Test]
        public void BuildAndSaveConfig3Test()
        {
            string url1 = "simu://";
            string url2 = "pdna://192.168.100.2";
            var list1 = new List<UeiDeviceInfo>();
            var list2 = new List<UeiDeviceInfo>();
            list1.Add(new UeiDeviceInfo(url1, 0, DeviceMap2.SimuAO16Literal));
            list2.Add(new UeiDeviceInfo(url2, 0, DeviceMap2.SimuAO16Literal));
            UeiBridge.LibraryA.CubeSetup cs1 = new UeiBridge.LibraryA.CubeSetup(list1);
            UeiBridge.LibraryA.CubeSetup cs2 = new UeiBridge.LibraryA.CubeSetup(list2);
            UeiBridge.LibraryA.Config2 c3 = new UeiBridge.LibraryA.Config2(new List<UeiBridge.LibraryA.CubeSetup> { cs1, cs2 });
            //c3.SavePerCube("UeiBridge", true);
        }
        [Test]
        public void LoadConfig3Test()
        {
            string cubeurl = "pdna://192.168.100.4";

            UeiDaq.DeviceCollection devColl = new UeiDaq.DeviceCollection(cubeurl);
            var l1 = devColl.Cast<UeiDaq.Device>().ToList();
            List<UeiDeviceInfo> devList = l1.Select( (UeiDaq.Device i) => 
            {
                if (i == null)
                    return null;
                return new UeiDeviceInfo(cubeurl, i.GetIndex(), i.GetDeviceName());
            }).ToList(); 

            UeiBridge.LibraryA.Config2 c2a = UeiBridge.LibraryA.Config2.LoadConfig( new List<List<UeiDeviceInfo>>() { devList });
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
        /// <summary>
        /// Verify that CubeSetup does not generate device-setup for an unknown device.
        /// </summary>
        [Test]
        public void CubeSetupTest1()
        {
            List<UeiDeviceInfo> devList = new List<UeiDeviceInfo>();
            devList.Add(new UeiDeviceInfo("cubeurl", 51,"devicename1"));
            CubeSetup cs = new CubeSetup(devList, "cubeurl");
            Assert.That( cs.DeviceSetupList.Count, Is.EqualTo(1));
        }
        /// <summary>
        /// Verify that CubeSetup does generate device-setup for a known device.
        /// </summary>
        [Test]
        public void CubeSetupTest2()
        {
            List<UeiDeviceInfo> devList = new List<UeiDeviceInfo>();
            devList.Add(new UeiDeviceInfo("cubeurl", 101,"AO-308"));
            CubeSetup cs = new CubeSetup(devList, "<unknown-url>");
            Assert.That( cs.DeviceSetupList.Count, Is.EqualTo(1));
            Assert.That(cs.DeviceSetupList[0], Is.Not.Null);
        }

        [Test]
        public void BuildDefaultSimuConfigTest()
        {
            Config2 c2 = new Config2();
            Config2 c3 = Config2.BuildDefaultConfig(new List<string>{ "simu://" });
            Assert.That( c3.AppSetup.StatusViewerEP, Is.Not.Null);
            Assert.That( c3.CubeSetupList[0].DeviceSetupList.Count, Is.EqualTo(11)); // only one simulation device setup is defined.
        }

        [Test]
        public void CompareConfigTest()
        {
            string filename = "setupForTest.config";

            Config2 c1 = Config2.BuildDefaultConfig(new List<string> { "simu://" });
            c1.SaveAs( new FileInfo(filename), true);

            Config2 c2 = Config2.LoadConfigFromFile(new FileInfo(filename));
            Config2 c3 = Config2.LoadConfigFromFile(new FileInfo(filename));
            c2.CubeSetupList.FirstOrDefault().DeviceSetupList.FirstOrDefault().DeviceName = "kkk";
            Assert.That(c2.Equals(c3), Is.False);
        }
        //[Test]
        public void FileInfoTest()
        {

            FileInfo fi = new FileInfo("file1.txt");

            CreateBackupFile(fi);

            using (StreamWriter sw = fi.AppendText())
            {
                sw.WriteLine("line1");
            }

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
