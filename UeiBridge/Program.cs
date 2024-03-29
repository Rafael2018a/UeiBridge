﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using log4net;
using UeiDaq;
using UeiBridge.Library.Types;
using UeiBridge.Library;
using UeiBridge.Library.CubeSetupTypes;
using UeiBridge.Library.Interfaces;

namespace UeiBridge
{
    public class Program
    {
        ILog _logger = StaticLocalMethods.GetLogger();
        ProgramObjectsBuilder _programBuilder;
        Config2 _mainConfig;
        bool stopByUser = false;

        static void Main(string[] args)
        {
            Program p = new Program();
            p.Run();

        }
        /// <summary>
        /// Build linear device list.
        /// This method assumes that the indicated cubes exists.
        /// </summary>
        public static List<UeiDeviceInfo> BuildLinearDeviceList(List<string> cubesUrl)
        {
            List<UeiDeviceInfo> resultList = new List<UeiDeviceInfo>();
            foreach (string url in cubesUrl)
            {
                UeiCube cube = new UeiCube(url);
                //DeviceCollection devColl = new DeviceCollection(url);
                resultList.AddRange(cube.GetDeviceInfoList()); //UeiBridge.Library.StaticMethods.DeviceCollectionToDeviceInfoList(devColl, url));
            }
            return resultList;
        }
        private void Run()
        {
            // print current version
            //var EntAsm = System.Reflection.Assembly.GetEntryAssembly();//.GetName().Version;
            var EntAsm = UeiBridge.Library.StaticMethods.GetLibVersion();
            System.IO.FileInfo fi = new System.IO.FileInfo(EntAsm.Location);
            _logger.Info($"UEI Bridge. Version {EntAsm.GetName().Version.ToString(4)}. Build time: {fi.LastWriteTime.ToString()}");

            // verify connected cubes
            List<string> cubeUrlList = GetConnectedCubes();
            if (cubeUrlList.Count == 0)
            {
                _logger.Warn("No connected cube found. Any key to abort....");
                Console.ReadKey();
                return;
            }

            Console.CancelKeyPress += new ConsoleCancelEventHandler(CancelEventHandler);
            Console.WriteLine("^C to stop");

            // load config
            try
            {
                List<CubeSetup> cubeSetupList = Config2.GetSetupForCubes(cubeUrlList); // tbd: very slow!
                // if setup file wasn't found, save default.
                foreach (CubeSetup cs in cubeSetupList)
                {
                    string fn = CubeSetup.GetSelfFilename(cs.GetCubeId());
                    
                    if (false == cs.IsLoadedFromFile)
                    {
                        FileInfo xfile = new FileInfo(fn);
                        CubeSetupLoader.SaveSetupFile(cs, xfile);
                        _logger.Info($"File {xfile.FullName } saved");
                    }
                }
                _mainConfig = new Config2(cubeUrlList); // tbd. very slow
            }
            catch( Exception ex)
            {
                _logger.Warn($"Error loading setup. {ex.Message}. Any key to abort....");
                Console.ReadKey();
                return;
            }

            List<UeiDeviceInfo> deviceList = BuildLinearDeviceList(cubeUrlList); // tbd: very slow

            _programBuilder = new ProgramObjectsBuilder( _mainConfig);
            _programBuilder.CreateDeviceManagers(deviceList);
            _programBuilder.ActivateDownstreamObjects();
            _programBuilder.ActivateUpstreamObjects();
#if dont
            _programBuilder.Build_BlockSensorManager(deviceList);
#endif

            // publish status to StatusViewer
            Task.Factory.StartNew(() => Task_PublishStatus(_programBuilder.PerDeviceObjectsList, _programBuilder.DeviceManagerList));

            // self tests
            //StartDownwardsTest();

            do
            {
                System.Threading.Thread.Sleep(100);
            } while (stopByUser == false);

            _logger.Info("Disposing....");
            _programBuilder.Dispose();

            _logger.Info("Any key to exit...");
            Console.ReadKey();
        }
        protected void CancelEventHandler(object sender, ConsoleCancelEventArgs args)
        {
            args.Cancel = true;
            stopByUser = true;
            Console.WriteLine("stopByUser1 = true");
        }

        /// <summary>
        /// if cubelist.txt file found, get url's from there 
        /// (this might include simu://)
        /// otherwise, search ip's in range
        /// </summary>
        private List<string> GetConnectedCubes()
        {
            FileInfo cubelistFile = new FileInfo("cubelist.txt");
            List<string> result = new List<string>();

            if (cubelistFile.Exists)
            {
                Console.WriteLine("Using cubelist.txt file");
                using (StreamReader fs = new StreamReader(cubelistFile.OpenRead()))
                {
                    while (true)
                    {
                        var l = fs.ReadLine();
                        if (null == l)
                        {
                            break;
                        }
                        Console.WriteLine($"Cube {l}");
                        result.Add(l);
                    }
                }
            }
            else
            {
                Console.WriteLine("Scanning for cubes...");
                List<IPAddress> iplist = CubeSeeker.FindCubesInRange(IPAddress.Parse("192.168.100.2"), 100);
                foreach (IPAddress ip in iplist)
                {
                    Console.WriteLine($"Found cube {ip.ToString()}");
                    result.Add($"pdna://{ip.ToString()}/");
                }
            }
            return result;
        }

        private void DisplayDeviceList(List<UeiDeviceInfo> devList)
        {
            // prepare device list
            if (null == devList) throw new ArgumentNullException();

            IEnumerable<IGrouping<string, UeiDeviceInfo>> GroupByUrl = devList.GroupBy(s => s.CubeUrl);

            foreach (IGrouping<string, UeiDeviceInfo> group in GroupByUrl)
            {
                _logger.Info($" *** Device list for cube {group.Key}:");
                foreach (UeiDeviceInfo dev in group)
                {
                    _logger.Info($"{dev.DeviceName} on slot {dev.DeviceSlot}");
                }
                //_logger.Info(" *** End device list:");
            }

        }
        void Task_PublishStatus(List<PerDeviceObjects> deviceList, List<IDeviceManager> deviceList2)
        {
            System.Threading.Thread.CurrentThread.Name = "Task:PublishStatus";

            //const int intervalMs = 100;
            IPEndPoint destEP = _mainConfig.AppSetup.StatusViewerEP.ToIpEp();
            UdpWriter uw = new UdpWriter( destEP, _mainConfig.AppSetup.SelectedNicForMulticast);
            TimeSpan interval = TimeSpan.FromMilliseconds(100);
            _logger.Info($"StatusViewer dest ep: {destEP.ToString()} (Local NIC {_mainConfig.AppSetup.SelectedNicForMulticast})");

            List<IDeviceManager> deviceListScan = new List<IDeviceManager>();

            try
            {

                // prepare list
                foreach (PerDeviceObjects deviceObjects in deviceList) //ProjectRegistry.Instance.OutputDevicesMap)
                {
                    if (deviceObjects.InputDeviceManager != null)
                    {
                        deviceListScan.Add(deviceObjects.InputDeviceManager);
                    }

                    if (deviceObjects?.OutputDeviceManager != null)
                    {
                        deviceListScan.Add(deviceObjects.OutputDeviceManager);
                    }
                }
                deviceListScan.AddRange(deviceList2);

                // get formatted string for each device in list
                while (true)
                {
                    foreach (IDeviceManager dm in deviceListScan)
                    {
                        string desc = $"{dm.InstanceName}";
                        StatusTrait tr = StatusTrait.IsRegular;
                        string[] stat = dm.GetFormattedStatus(interval);
                        if (null == stat)
                        {
                            continue;
                        }
                        StatusEntryJson js = new StatusEntryJson(desc, stat, tr);
                        string s = Newtonsoft.Json.JsonConvert.SerializeObject(js);
                        byte[] send_buffer = Encoding.ASCII.GetBytes(s);
                        SendObject so = new SendObject(destEP, send_buffer);
                        uw.Send(so);
                    }

                    System.Threading.Thread.Sleep(interval);
                }
            }
            catch ( Exception ex)
            {
                _logger.Warn(ex.Message);
            }
        }

        private void StartDownwardsTest()
        {
            Task.Factory.StartNew(() =>
            {
                _logger.Info("Downward message simulation active.");

                try
                {
                    UdpClient udpClient = new UdpClient();
                    System.Threading.Thread.Sleep(100);

                    for (int i = 0; i < 10; i++)
                    {
                        // digital out
                        {
                            //IPEndPoint destEp = Config2.Instance.UeiCubes[0].DeviceSetupList[5].LocalEndPoint?.ToIpEp();
                            //byte[] e403 = StaticMethods.Make_DIO403Down_Message();
                            //udpClient.Send(e403, e403.Length, destEp);
                        }
                        // analog out
                        {
                            //IPEndPoint destEp = Config2.Instance.UeiCubes[0].DeviceSetupList[0].LocalEndPoint.ToIpEp();
                            //byte[] e308 = StaticMethods.Make_A308Down_message();
                            //udpClient.Send(e308, e308.Length, destEp);
                        }
#if dontremove
                        byte[] e430 = StaticMethods.Make_DIO430Down_Message();
                        udpClient.Send(e430, e308.Length, destEp);
#endif


                        // serial out
                        List<byte[]> e508 = Library.StaticMethods.Make_SL508Down_Messages(i);
                        foreach (byte[] msg in e508)
                        {
                            //IPEndPoint destEp = Config2.Instance.UeiCubes[0].DeviceSetupList[3].LocalEndPoint.ToIpEp();
                            //udpClient.Send(msg, msg.Length, destEp);
                            //System.Threading.Thread.Sleep(10);
                        }

                        // relays
                        {
                            //byte[] e470 = StaticMethods.Make_DIO470_Down_Message();
                            //IPEndPoint destEp = Config2.Instance.UeiCubes[0].DeviceSetupList[4].LocalEndPoint.ToIpEp();
                            //udpClient.Send(e470, e470.Length, destEp);
                        }

                        // block sensor
                        {
                            //IPEndPoint destEp = _mainConfig.Blocksensor.LocalEndPoint.ToIpEp();
                            //EthernetMessage em = Library.StaticMethods.Make_BlockSensor_downstream_message(new Int16[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 });
                            //udpClient.Send(em.GetByteArray(MessageWay.downstream), em.GetByteArray(MessageWay.downstream).Length, destEp);
                        }

                        Thread.Sleep(1000);

                    }
                    _logger.Info("Downward message simulation end");

                }
                catch (Exception ex)
                {
                    _logger.Warn(ex.Message);
                }
            });
        }

    }
}
