using System;
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
using UeiBridge.Types;
using UeiBridge.Library;

namespace UeiBridge
{
    public class Program
    {
        ILog _logger = StaticMethods.GetLogger();
        //List<InputDevice> _inputDevices = new List<InputDevice>();
        ProgramObjectsBuilder _programBuilder = new ProgramObjectsBuilder();

        static void Main(string[] args)
        {
            Program p = new Program();
            p.Run();

            System.Threading.Thread.Sleep(1000);
        }

        public static List<DeviceEx> BuildDeviceList(List<string> cubesUrl)
        {
            List<DeviceEx> resultList = new List<DeviceEx>();
            foreach (string url in cubesUrl)
            {
                DeviceCollection devColl = new DeviceCollection(url);

                foreach (Device dev in devColl)
                {
                    if (dev == null) continue; // this for the last entry, which is null
                    resultList.Add(new DeviceEx(dev, url));
                }
            }
            return resultList;
        }

        

        private void Run()
        {
            // print current version
            var EntAsm = System.Reflection.Assembly.GetEntryAssembly();//.GetName().Version;
            System.IO.FileInfo fi = new System.IO.FileInfo(EntAsm.Location);
            _logger.Info($"UEI Bridge. Version {EntAsm.GetName().Version.ToString(3)}. Build time: {fi.LastWriteTime.ToString()}");

            List<string> cubesUrl = GetConnectedCubes();
            if (!Config2.IsConfigFileExist())
            {
                Config2.Instance.BuildNewConfig(cubesUrl.ToArray());
            }
            List<DeviceEx> deviceList = BuildDeviceList( cubesUrl);

            bool ok = DisplayDeviceList( deviceList);
            if (!ok)
            {
                _logger.Info("Any key to exit...");
                Console.ReadKey();
                return;
            }

            _programBuilder.CreateDeviceManagers(deviceList);
            _programBuilder.ActivateDownstreamOjects();
            _programBuilder.ActivateUpstreamObjects();
            _programBuilder.CreateBlockSensorManager(deviceList);


            //BuildProgramObjects();
           
            // publish status to StatusViewer
            Task.Factory.StartNew(() => PublishStatus_Task(_programBuilder.DeviceManagers));

            // self tests
            //StartDownwardsTest();

            _logger.Info("Any key to exit...");
            Console.ReadKey();

            _logger.Info("Disposing....");
            //DisposeProgramObjects2();

            _logger.Info("Any key to exit...");
            Console.ReadKey();
        }

        private List<string> GetConnectedCubes()
        {
            FileInfo cubelistFile = new FileInfo("cubelist.txt");
            List<string> result = new List<string>();

            if (cubelistFile.Exists)
            {
                using (StreamReader fs = new StreamReader(cubelistFile.OpenRead()))
                {
                    while (true)
                    {
                        var l = fs.ReadLine();
                        if (null == l)
                        {
                            break;
                        }
                        result.Add(l);
                    }
                }
            }
            else
            {
                List<IPAddress> iplist = CubeSeeker.FindCubesInRange(IPAddress.Parse("192.168.100.2"), 100);
                foreach (IPAddress ip in iplist)
                {
                    result.Add( $"pdna://{ip.ToString()}/");
                }
            }
            return result;
        }

        //private void DisposeProgramObjest_old()
        //{
        //    for (int cubeIndex = 0; cubeIndex < _deviceObjectsTable.Count; cubeIndex++)
        //    {
        //        var dl = _deviceObjectsTable[cubeIndex];
        //        for (int slot = 0; slot < dl.Count; slot++)
        //        {
        //            if (null != dl[slot])
        //            {
        //                dl[slot].OutputDeviceManager.Dispose();
        //            }
        //        }
        //    }
        //}
#if dont
        /// <summary>
        /// Display devices in all cubes
        /// </summary>
        /// <returns>true on success</returns>
        private bool DisplayDeviceList() // tbd: show for ALL cubes
        {
            // prepare device list
            List<Device> deviceList = StaticMethods.GetDeviceList("TBD");
            if (null == deviceList)
            {
                _logger.Error(StaticMethods.LastErrorMessage);
                return false;
            }
            if (0 == deviceList.Count)
            {
                _logger.Warn("No device connected");
                return false;
            }

            // display device list
            _logger.Info(" *** Device list:");
            //deviceList.ForEach(dev => _logger.Info($"{dev.GetDeviceName()} as Dev{dev.GetIndex()}"));
            foreach (var dev in deviceList)
            {
                _logger.Info($"{dev.GetDeviceName()} as Dev{dev.GetIndex()}");
            }
            _logger.Info(" *** End device list:");

            return true;
        }
#endif
        private bool DisplayDeviceList( List<DeviceEx> devList) // tbd: show for ALL cubes
        {
            // prepare device list
            if (null == devList) throw new ArgumentNullException();

            IEnumerable<IGrouping<string, DeviceEx>> GroupByUrl = devList.GroupBy(s => s.CubeUrl);

            foreach (IGrouping<string, DeviceEx> group in GroupByUrl)
            {
                _logger.Info($" ====== Device list for cube {group.Key}:");
                foreach (DeviceEx dev in group)
                {
                    _logger.Info($"{dev.PhDevice.GetDeviceName()} on slot {dev.PhDevice.GetIndex()}");
                }
                //_logger.Info(" *** End device list:");
            }

            return true;
        }
#if old
        //List<List<OutputDevice>> _outputDeviceList;
        List<List<PerDeviceObjects>> _deviceObjectsTable;
        /// <summary>
        /// 
        /// </summary>

        private void BuildProgramObjects()
        {
            // prepare lists
            int noOfCubes = 1;// Config2.Instance.CubeUrlList.Length;
            _deviceObjectsTable = new List<List<PerDeviceObjects>>(new List<PerDeviceObjects>[noOfCubes]);

            System.Diagnostics.Debug.Assert(Config2.Instance.UeiCubes.Length == 1); // only one cube handled!
            // Create program Objects
            foreach (var cubeSetup in Config2.Instance.UeiCubes)
            {
                // init device list
                List<UeiDaq.Device> realDeviceList = StaticMethods.GetDeviceList(cubeSetup.CubeUrl);
                _deviceObjectsTable[cubeSetup.CubeNumber] = new List<PerDeviceObjects>();
                int slot = 0;
                foreach ( UeiDaq.Device dev in realDeviceList) 
                {
                    System.Diagnostics.Debug.Assert(slot == dev.GetIndex());
                    _deviceObjectsTable[cubeSetup.CubeNumber].Add(new PerDeviceObjects(dev.GetDeviceName(), dev.GetIndex(), ""));
                    ++slot;
                }

                CreateSerialSessions(cubeSetup, _deviceObjectsTable);
                CreateDownwardsObjects(cubeSetup, _deviceObjectsTable);
                CreateUpwardsObjects(cubeSetup, _deviceObjectsTable);

                System.Threading.Thread.Sleep(100);

                ActivateDownwardOjects(_deviceObjectsTable[cubeSetup.CubeNumber]);
                ActivateUpwardObjects(_deviceObjectsTable[cubeSetup.CubeNumber]);

                // special treatment to blockSenser which is 'an OutputDevice'
                BlockSensorManager blockSensor = CreateBlockSensorObject(realDeviceList, Config2.Instance.Blocksensor);
                if (null != blockSensor)
                {
                    var x1 = _deviceObjectsTable[cubeSetup.CubeNumber].Where(i => i.InputDeviceManager!=null);
                    var x2 = x1.Where( i => i.InputDeviceManager.DeviceName.StartsWith("DIO")).Select(i => i.InputDeviceManager).FirstOrDefault();
                    DIO403InputDeviceManager di = x2 as DIO403InputDeviceManager;
                    di.TargetConsumer = blockSensor;

                    var nic = IPAddress.Parse(Config2.Instance.AppSetup.SelectedNicForMCast);
                    UdpReader ureader = new UdpReader(Config2.Instance.Blocksensor.LocalEndPoint.ToIpEp(), nic, blockSensor, "BlockSensor");
                    _deviceObjectsTable[cubeSetup.CubeNumber].Add(new PerDeviceObjects("BlockSensor", -1, ""));
                    _deviceObjectsTable[cubeSetup.CubeNumber].Last().Update(blockSensor, ureader, -1);
                    blockSensor.OpenDevice();
                    ureader.Start();
                }
            }

            // Activate program Objects
            //foreach (List<PerDeviceObjects> cubeSetup in _deviceObjectsTable)
            //{
            //    ActivateDownwardOjects(cubeSetup);
            //    ActivateUpwardObjects(cubeSetup);
            //}
        }

        private static void ActivateDownwardOjects(List<PerDeviceObjects> deviceObjectsList)
        {
            // activate downward (output) objects
            foreach (PerDeviceObjects deviceObjects in deviceObjectsList)
            {
                if (null != deviceObjects)
                {
                    if (null != deviceObjects.OutputDeviceManager)
                    {
                        deviceObjects.OutputDeviceManager.OpenDevice();
                        deviceObjects.UdpReader.Start();
                    }
                    //if (null != deviceObjects.BlockSensor)
                    //{
                    //    deviceObjects.BlockSensor.Start();
                    //}
                    Thread.Sleep(10);
                }
            }
        }
        private static void ActivateUpwardObjects(List<PerDeviceObjects> deviceObjectsList)
        {
            // activate upward (input) objects
            foreach (PerDeviceObjects deviceObjects in deviceObjectsList)
            {
                deviceObjects?.InputDeviceManager?.OpenDevice();
                Thread.Sleep(10);
                // (no need to activate udpWriter)
            }
        }

        [Obsolete]
        void DisposeProgramObjects()
        {
            throw new ApplicationException("obsolete");

            for (int cubeIndex = 0; cubeIndex < _deviceObjectsTable.Count; cubeIndex++)
            {
                List<PerDeviceObjects> devList = _deviceObjectsTable[cubeIndex];

                for (int deviceIndex = 0; deviceIndex < devList.Count; deviceIndex++)
                {
                    _logger.Debug($"Disposing Slot {deviceIndex}");
                    // dispose upward object
                    devList[deviceIndex]?.InputDeviceManager?.Dispose();
                    // dispose downward object
                    devList[deviceIndex]?.UdpReader?.Dispose();
                    devList[deviceIndex]?.OutputDeviceManager?.Dispose();

                    // dispose serial session object
                    devList[deviceIndex]?.SerialSession?.Dispose();
                }
            }
        }
        void DisposeProgramObjects2()
        {
            for (int cubeIndex = 0; cubeIndex < _deviceObjectsTable.Count; cubeIndex++)
            {
                // first, shut down all inputs
                foreach (PerDeviceObjects objs in _deviceObjectsTable[cubeIndex])
                {
                    objs.UdpReader?.Dispose();
                }
                // next, shut down upstream devices
                foreach (PerDeviceObjects objs in _deviceObjectsTable[cubeIndex])
                {
                    objs.InputDeviceManager?.Dispose();
                }
                // finally, downstream objects
                foreach (PerDeviceObjects objs in _deviceObjectsTable[cubeIndex])
                {
                    objs.OutputDeviceManager?.Dispose();
                    objs.SerialSession?.Dispose();
                    objs.UdpWriter?.Dispose();
                }
            }

                //for (int deviceIndex = 0; deviceIndex < devList.Count; deviceIndex++)
                //{
                //    _logger.Debug($"Disposing Slot {deviceIndex}");
                //    // dispose upward object
                //    devList[deviceIndex]?.InputDeviceManager?.Dispose();
                //    // dispose downward object
                //    devList[deviceIndex]?.UdpReader?.Dispose();
                //    devList[deviceIndex]?.OutputDeviceManager?.Dispose();

                //    // dispose serial session object
                //    devList[deviceIndex]?.SerialSession?.Dispose();
                //}
            
        }

        /// <summary>
        /// Create output device managers and udp readers
        /// </summary>
        private void CreateDownwardsObjects(CubeSetup cubeSetup, List<List<PerDeviceObjects>> deviceObjectsTable)
        {
            List<UeiDaq.Device> realDeviceList = StaticMethods.GetDeviceList(cubeSetup.CubeUrl);

            // populate outputDeviceList
            foreach (UeiDaq.Device realDevice in realDeviceList)
            {
                int realSlot = realDevice.GetIndex();
                DeviceSetup deviceSetup = Config2.Instance.UeiCubes[cubeSetup.CubeNumber].DeviceSetupList[realSlot]; // tbd: first 'DeviceSetup' in config is not neccesseraly in slot 0
                deviceSetup.CubeUrl = cubeSetup.CubeUrl;
                System.Diagnostics.Debug.Assert(realSlot == deviceSetup.SlotNumber);
                if (realDevice.GetDeviceName() != deviceSetup.DeviceName)
                {
                    Console.WriteLine($"Slot{realSlot}: Card of type {realDevice.GetDeviceName()} does not match config entry of type {deviceSetup.DeviceName}. Skipping card.");
                    continue;
                }

                Type devType = StaticMethods.GetDeviceManagerType<OutputDevice>(deviceSetup.DeviceName);
                if (null == devType) // if no device-manager class support this device
                {
                    //_logger.Debug($"Output device of type {deviceSetup.DeviceName} not supported");
                    continue;
                }

                // create device-manager instance
                OutputDevice outDev;
                if (devType.Name.StartsWith("SL508")) // special treatment to serial device
                {
                    outDev = (OutputDevice)Activator.CreateInstance(devType, deviceSetup, deviceObjectsTable[cubeSetup.CubeNumber][realSlot].SerialSession);
                }
                else
                {
                    outDev = (OutputDevice)Activator.CreateInstance(devType, deviceSetup);
                }

                System.Diagnostics.Debug.Assert(null != outDev);
                System.Diagnostics.Debug.Assert(null != deviceSetup.LocalEndPoint);

                // create udp reader for this device
                var nic = IPAddress.Parse(Config2.Instance.AppSetup.SelectedNicForMCast);
                UdpReader ureader = new UdpReader(deviceSetup.LocalEndPoint.ToIpEp(), nic, outDev, outDev.InstanceName);

                // update device table
                deviceObjectsTable[cubeSetup.CubeNumber][realSlot].Update( outDev, ureader, realSlot);
            }
        }

        /// <summary>
        /// Create input-device managers and udp-writers
        /// </summary>
        /// <param name="cubeSetup"></param>
        /// <param name="deviceObjectsTable"></param>
        private void CreateUpwardsObjects(CubeSetup cubeSetup, List<List<PerDeviceObjects>> deviceObjectsTable)
        {
            List<UeiDaq.Device> realDeviceList = StaticMethods.GetDeviceList(cubeSetup.CubeUrl);

            // search for blocksensor, this affect creation of DIO403Input
            var x = _deviceObjectsTable[cubeSetup.CubeNumber].Where(d => { return d.OutputDeviceManager?.DeviceName.StartsWith("BlockSensor") == true; }).Select(d => d.OutputDeviceManager);
            OutputDevice blockSensor = x.FirstOrDefault() as OutputDevice;

            // Create input-devices instances
            foreach (UeiDaq.Device realDevice in realDeviceList)
            {
                int realSlot = realDevice.GetIndex();
                DeviceSetup deviceSetup = Config2.Instance.UeiCubes[cubeSetup.CubeNumber].DeviceSetupList[realSlot]; // tbd: first 'DeviceSetup' in config is not necessarily in slot 0
                deviceSetup.CubeUrl = cubeSetup.CubeUrl; // for later use
                System.Diagnostics.Debug.Assert(realSlot == deviceSetup.SlotNumber);

                if (realDevice.GetDeviceName() != deviceSetup.DeviceName)
                {
                    Console.WriteLine($"Slot{realSlot}: Card of type {realDevice.GetDeviceName()} does not match config entry of type {deviceSetup.DeviceName}. Skipping card.");
                    continue;
                }


                Type devType = StaticMethods.GetDeviceManagerType<InputDevice>(deviceSetup.DeviceName);
                if (null == devType) // if no device-manager-class supports this device
                {
                    //_logger.Debug($"Input device of type {deviceSetup.DeviceName} not supported");
                    continue;
                }

                string instanceName = $"{realDevice.GetDeviceName()}/Slot{deviceSetup.SlotNumber}";
                UdpWriter uWriter = new UdpWriter(instanceName, deviceSetup.DestEndPoint.ToIpEp(), Config2.Instance.AppSetup.SelectedNicForMCast);
                InputDevice inDev;
                if (devType.Name.StartsWith("SL508")) // special treatment for serial device
                {
                    inDev = (InputDevice)Activator.CreateInstance(devType, uWriter, deviceSetup, deviceObjectsTable[cubeSetup.CubeNumber][realSlot].SerialSession);
                }
                else
                {
                    inDev = (InputDevice)Activator.CreateInstance(devType, uWriter, deviceSetup);
                }

                deviceObjectsTable[cubeSetup.CubeNumber][realSlot].Update(inDev, uWriter, realSlot);
            }
        }

        /// <summary>
        /// BlockSensorManager might be created only if digital and analog cards exists
        /// </summary>
        private BlockSensorManager CreateBlockSensorObject(List<Device> realDeviceList, BlockSensorSetup blockSensorSetup)
        {
            if (false == blockSensorSetup.IsActive)
            {
                _logger.Debug("Block sensor disabled.");
                return null;
            }

            BlockSensorManager result = null;
            var x = _deviceObjectsTable[0].Where(d => (d.OutputDeviceManager != null) && d.OutputDeviceManager.DeviceName.StartsWith("AO-308")).Select(d => d.OutputDeviceManager);
            System.Diagnostics.Debug.Assert(x.Count() > 0);
            AO308OutputDeviceManager ao308 = x.FirstOrDefault() as AO308OutputDeviceManager;
            System.Diagnostics.Debug.Assert(null != ao308);
            bool digitalExist = realDeviceList.Any(d => d.GetDeviceName().StartsWith("DIO-403"));
            if (digitalExist && ao308!=null)
            {
                result = new BlockSensorManager(blockSensorSetup, ao308.AnalogWriter);
                _logger.Info("Blocksensor object created");
            }
            else
            {
                _logger.Warn("Failed to create blocksensor object");
            }
            return result;
        }

        void CreateSerialSession2( List<DeviceEx> realDeviceList)
        {
            var serials = realDeviceList.Where(i => i.PhDevice.GetDeviceName().StartsWith("SL508"));
        }

        /// <summary>
        /// Create serial session and assign it to appropriate entry 
        /// </summary>
        /// <param name="cubeSetup"></param>
        /// <param name="deviceObjectsTable"></param>
        private void CreateSerialSessions(CubeSetup cubeSetup, List<List<PerDeviceObjects>> deviceObjectsTable)
        {
            List<UeiDaq.Device> realDeviceList = StaticMethods.GetDeviceList(cubeSetup.CubeUrl);

            // populate outputDeviceList
            foreach (UeiDaq.Device realDevice in realDeviceList)
            {
                int realSlot = realDevice.GetIndex();
                DeviceSetup deviceSetup = Config2.Instance.UeiCubes[cubeSetup.CubeNumber].DeviceSetupList[realSlot];
                System.Diagnostics.Debug.Assert(realSlot == deviceSetup.SlotNumber); // first 'DeviceSetup' entry in config must be at slot 0 etc..
                deviceSetup.CubeUrl = cubeSetup.CubeUrl;
                if (realDevice.GetDeviceName() != deviceSetup.DeviceName)
                {
                    //Console.WriteLine($"Slot{realSlot}: Card of type {realDevice.GetDeviceName()} does not match config entry of type {deviceSetup.DeviceName}. Skipping card.");
                    continue;
                }

                Type devType = StaticMethods.GetDeviceManagerType<OutputDevice>(deviceSetup.DeviceName);
                if (null == devType)
                {
                    continue;
                }

                //OutputDevice outDev;
                if (devType.Name.StartsWith("SL508"))
                {
                    System.Diagnostics.Debug.Assert(null == deviceObjectsTable[cubeSetup.CubeNumber][realSlot].SerialSession);
                    SL508Session serialSession = new SL508Session(deviceSetup as SL508892Setup);
                    System.Diagnostics.Debug.Assert(null != serialSession);
                    deviceObjectsTable[cubeSetup.CubeNumber][realSlot].Update(serialSession, realSlot);
                }
            }
        }
#endif
        void PublishStatus_Task( List<PerDeviceObjects> deviceList)
        {
            const int intervalMs = 100;
            IPEndPoint destEP = Config2.Instance.AppSetup.StatusViewerEP.ToIpEp();
            UdpWriter uw = new UdpWriter("To-StatusViewer", destEP, Config2.Instance.AppSetup.SelectedNicForMCast);
            TimeSpan interval = TimeSpan.FromMilliseconds(intervalMs);
            _logger.Info($"StatusViewer dest ep: {destEP.ToString()} (Local NIC {Config2.Instance.AppSetup.SelectedNicForMCast})");

            List<IDeviceManager> deviceListScan = new List<IDeviceManager>();

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

            // get formatted string for each device in list
            while (true)
            {
                foreach (IDeviceManager dm in deviceListScan)
                {
                    string desc = $"{dm.InstanceName}";
                    StatusTrait tr = StatusTrait.IsRegular;
                    string[] stat = dm.GetFormattedStatus(interval);
                    StatusEntryJson js = new StatusEntryJson(desc, stat, tr);
                    string s = Newtonsoft.Json.JsonConvert.SerializeObject(js);
                    byte[] send_buffer = Encoding.ASCII.GetBytes(s);
                    SendObject so = new SendObject(destEP, send_buffer);
                    uw.Send(so);
                }

                System.Threading.Thread.Sleep(interval);
            }

            //while (true)
            //{
            //    foreach (PerDeviceObjects deviceObjects in _deviceObjectsTable[0]) //ProjectRegistry.Instance.OutputDevicesMap)
            //    {
            //        if (deviceObjects?.InputDeviceManager!=null)
            //        {
            //            IDeviceManager dm = deviceObjects.InputDeviceManager;
            //            SendToViewer(destEP, uw, dm);
            //        }

            //        if(deviceObjects?.OutputDeviceManager!=null)
            //        {
            //            IDeviceManager dm = deviceObjects.OutputDeviceManager;
            //            SendToViewer(destEP, uw, dm);

            //        }
            //    }


            //}
        }

        //private static void SendToViewer(IPEndPoint destEP, UdpWriter uw, IDeviceManager dm)
        //{
        //    string desc = $"{dm.InstanceName}";
        //    string stat = dm.GetFormattedStatus( );
        //    UeiLibrary.JsonStatusClass js = new UeiLibrary.JsonStatusClass(desc, stat);
        //    string s = Newtonsoft.Json.JsonConvert.SerializeObject(js);
        //    byte[] send_buffer = Encoding.ASCII.GetBytes(s);
        //    SendObject so = new SendObject(destEP, send_buffer);
        //    uw.Send(so);
        //}

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
                            IPEndPoint destEp = Config2.Instance.Blocksensor.LocalEndPoint.ToIpEp();
                            EthernetMessage em = Library.StaticMethods.Make_BlockSensor_downstream_message(new UInt16[]{ 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 });
                            udpClient.Send(em.GetByteArray( MessageWay.downstream), em.GetByteArray(MessageWay.downstream).Length, destEp);
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
#if dont
        private byte[] Make_AO308_Message_old()
        {
            byte[] eth = new byte[32];
            Array.Clear(eth, 0, eth.Length);

            eth[0] = 0xAA;
            eth[1] = 0x55;

            Int16 val = 0;
            for (int i = 0; i < 8; i++)
            {
                byte[] bytes = BitConverter.GetBytes(val);
                bytes.CopyTo(eth, 16 + i * 2);
                val += 2000;
            }
            Int16 len = 32;
            byte[] bytes1 = BitConverter.GetBytes(len);
            bytes1.CopyTo(eth, 12);

            return eth;
        }

        internal PerDeviceObjects PerDeviceObjects
        {
            get => default;
            set
            {
            }
        }
#endif

    }
}
