using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using log4net;
using UeiDaq;
using UeiBridge.Types;
using UeiBridge.Library;

namespace UeiBridge
{
    class Program
    {
        ILog _logger = StaticMethods.GetLogger();
        List<InputDevice> _inputDevices = new List<InputDevice>();

        static void Main(string[] args)
        {
            Program p = new Program();
            p.Run();

            System.Threading.Thread.Sleep(1000);
        }

        private void Run()
        {
            // print current version
            var v = System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString();
            _logger.Info($"UEI Bridge. Version {v} Device URL: {Config2.Instance.CubeUrlList[0]}");

            bool ok = DisplayDeviceList();
            if (!ok)
            {
                _logger.Info("Any key to exit...");
                Console.ReadKey();
                return;
            }

            BuildProgramObjects();

            // publish status to StatusViewer
            Task.Factory.StartNew(() => PublishStatus_Task());

            // self tests
            //StartDownwardsTest();

            _logger.Info("Any key to exit...");
            Console.ReadKey();

            _logger.Info("Disposing....");
            DisposeProgramObjects();

            _logger.Info("Any key to exit...");
            Console.ReadKey();
        }

        private void DisposeProgramObjest_old()
        {
            for (int cubeIndex = 0; cubeIndex < _deviceObjectsTable.Count; cubeIndex++)
            {
                var dl = _deviceObjectsTable[cubeIndex];
                for (int slot = 0; slot < dl.Count; slot++)
                {
                    if (null != dl[slot])
                    {
                        dl[slot].OutputDeviceManager.Dispose();
                    }
                }
            }
        }

        /// <summary>
        /// Display devices in all cubes
        /// </summary>
        /// <returns>true on success</returns>
        private bool DisplayDeviceList() // tbd: show for ALL cubes
        {
            // prepare device list
            List<Device> deviceList = StaticMethods.GetDeviceList(Config2.Instance.CubeUrlList[0]);
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

        //List<List<OutputDevice>> _outputDeviceList;
        List<List<PerDeviceObjects>> _deviceObjectsTable;
        /// <summary>
        /// 
        /// </summary>
        private void BuildProgramObjects()
        {
            // prepare lists
            int noOfCubes = Config2.Instance.CubeUrlList.Length;
            _deviceObjectsTable = new List<List<PerDeviceObjects>>(new List<PerDeviceObjects>[noOfCubes]);

            // Create program Objects
            foreach (var cubeSetup in Config2.Instance.UeiCubes)
            {
                // init device list
                List<UeiDaq.Device> realDeviceList = StaticMethods.GetDeviceList(cubeSetup.CubeUrl);
                _deviceObjectsTable[cubeSetup.CubeNumber] = new List<PerDeviceObjects>();
                for (int i = 0; i < realDeviceList.Count + 1; i++) // add 1 for blocksensor
                {
                    _deviceObjectsTable[cubeSetup.CubeNumber].Add(new PerDeviceObjects());
                }

                // special treatment to blockSenser which is 'an OutputDevice'

                var bsSetup = new DeviceSetup(null, null, null);
                BlockSensorManager blockSensor = CreateBlockSensorObject(realDeviceList, bsSetup);
                if (null != blockSensor)
                {
                    var nic = IPAddress.Parse(Config2.Instance.AppSetup.SelectedNicForMCast);
                    UdpReader ureader = new UdpReader(bsSetup.LocalEndPoint.ToIpEp(), nic, blockSensor, "blocksensor");
                    var pdo = new PerDeviceObjects(blockSensor, ureader);
                    _deviceObjectsTable[cubeSetup.CubeNumber].Add(pdo);
                }
                ///

                CreateSerialSessions(cubeSetup, _deviceObjectsTable);
                CreateDownwardsObjects(cubeSetup, _deviceObjectsTable);
                CreateUpwardsObjects(cubeSetup, _deviceObjectsTable);

                // attach ao308 to blocksensor
                if (null != blockSensor)
                {
                    var x = _deviceObjectsTable[cubeSetup.CubeNumber].Where(d => (d.OutputDeviceManager != null) && d.OutputDeviceManager.DeviceName.StartsWith("AO308")).Select(d => d.OutputDeviceManager);
                    AO308OutputDeviceManager ao308 = x.First() as AO308OutputDeviceManager;
                    System.Diagnostics.Debug.Assert(null != ao308);
                    blockSensor.SetAnalogOuputInterface(ao308);
                }
            }

            // Activate program Objects
            foreach (List<PerDeviceObjects> cubeSetup in _deviceObjectsTable)
            {
                ActivateDownwardOjects(cubeSetup);
                ActivateUpwardObjects(cubeSetup);
            }
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

        void DisposeProgramObjects()
        {
            for (int cubeIndex = 0; cubeIndex < _deviceObjectsTable.Count; cubeIndex++)
            {
                List<PerDeviceObjects> devList = _deviceObjectsTable[cubeIndex];

                for (int deviceIndex = 0; deviceIndex < devList.Count; deviceIndex++)
                {
                    _logger.Debug($"Disposing Slot {deviceIndex}");
                    // dispose upword object
                    devList[deviceIndex]?.InputDeviceManager?.Dispose();
                    // dispose downword object
                    devList[deviceIndex]?.UdpReader?.Dispose();
                    devList[deviceIndex]?.OutputDeviceManager?.Dispose();

                    // dispose serial session object
                    devList[deviceIndex]?.SerialSession?.Dispose();
                }
            }
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

                // add instances to device vector
                deviceObjectsTable[cubeSetup.CubeNumber][realSlot].OutputDeviceManager = outDev;
                deviceObjectsTable[cubeSetup.CubeNumber][realSlot].UdpReader = ureader;
            }


        }

        /// <summary>
        /// Creaete input-device managers and udp-writers
        /// </summary>
        /// <param name="cubeSetup"></param>
        /// <param name="deviceObjectsTable"></param>
        private void CreateUpwardsObjects(CubeSetup cubeSetup, List<List<PerDeviceObjects>> deviceObjectsTable)
        {
            List<UeiDaq.Device> realDeviceList = StaticMethods.GetDeviceList(cubeSetup.CubeUrl);

            // search for blocksensor, this affcet creation of DIO403Input

            var x = _deviceObjectsTable[cubeSetup.CubeNumber].Where(d => d.OutputDeviceManager.DeviceName.StartsWith("Block") == true).Select(d => d.OutputDeviceManager);
            OutputDevice blockSensor = x.First() as OutputDevice;

            // Create input-devices instances
            foreach (UeiDaq.Device realDevice in realDeviceList)
            {
                int realSlot = realDevice.GetIndex();
                DeviceSetup deviceSetup = Config2.Instance.UeiCubes[cubeSetup.CubeNumber].DeviceSetupList[realSlot]; // tbd: first 'DeviceSetup' in config is not neccesseraly in slot 0
                deviceSetup.CubeUrl = cubeSetup.CubeUrl; // for later use
                System.Diagnostics.Debug.Assert(realSlot == deviceSetup.SlotNumber);

                if (realDevice.GetDeviceName() != deviceSetup.DeviceName)
                {
                    Console.WriteLine($"Slot{realSlot}: Card of type {realDevice.GetDeviceName()} does not match config entry of type {deviceSetup.DeviceName}. Skipping card.");
                    continue;
                }


                Type devType = StaticMethods.GetDeviceManagerType<InputDevice>(deviceSetup.DeviceName);
                if (null == devType) // if no device-manager-class supports thid device
                {
                    continue;
                }

                string instanceName = $"{realDevice.GetDeviceName()}/Slot{deviceSetup.SlotNumber}";
                UdpWriter uWriter = new UdpWriter(instanceName, deviceSetup.DestEndPoint.ToIpEp(), Config2.Instance.AppSetup.SelectedNicForMCast);
                InputDevice inDev;
                if (devType.Name.StartsWith("SL508")) // special treatment to serial device
                {
                    inDev = (InputDevice)Activator.CreateInstance(devType, uWriter, deviceSetup, deviceObjectsTable[cubeSetup.CubeNumber][realSlot].SerialSession);
                }
                else if ((null != blockSensor) && (devType.Name.StartsWith("DIO403In")))
                {
                    TeeObject tee = new TeeObject(blockSensor, uWriter);
                    inDev = (InputDevice)Activator.CreateInstance(devType, tee, deviceSetup);
                }
                else
                {
                    inDev = (InputDevice)Activator.CreateInstance(devType, uWriter, deviceSetup);
                }

                deviceObjectsTable[cubeSetup.CubeNumber][realSlot].InputDeviceManager = inDev;
                deviceObjectsTable[cubeSetup.CubeNumber][realSlot].UdpWriter = uWriter;
            }
        }

        private BlockSensorManager CreateBlockSensorObject(List<Device> realDeviceList, DeviceSetup analogSensorSetup)
        {
            throw new NotImplementedException();
        }

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
                    deviceObjectsTable[cubeSetup.CubeNumber][realSlot].SerialSession = serialSession;
                }
            }
        }

        void PublishStatus_Task()
        {
            const int intervalMs = 100;
            IPEndPoint destEP = Config2.Instance.AppSetup.StatusViewerEP.ToIpEp();
            UdpWriter uw = new UdpWriter("To-StatusViewer", destEP, Config2.Instance.AppSetup.SelectedNicForMCast);
            TimeSpan interval = TimeSpan.FromMilliseconds(intervalMs);
            _logger.Info($"StatusViewer dest ep: {destEP.ToString()}");

            List<IDeviceManager> deviceList = new List<IDeviceManager>();

            foreach (PerDeviceObjects deviceObjects in _deviceObjectsTable[0]) //ProjectRegistry.Instance.OutputDevicesMap)
            {
                if (deviceObjects?.InputDeviceManager != null)
                {
                    deviceList.Add(deviceObjects.InputDeviceManager);
                }

                if (deviceObjects?.OutputDeviceManager != null)
                {
                    deviceList.Add(deviceObjects.OutputDeviceManager);
                }
            }

            while (true)
            {
                foreach (IDeviceManager dm in deviceList)
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

                    for (int i = 0; i < 1; i++)
                    {
                        // digital out
                        {
                            IPEndPoint destEp = Config2.Instance.UeiCubes[0].DeviceSetupList[5].LocalEndPoint?.ToIpEp();
                            byte[] e403 = StaticMethods.Make_DIO403Down_Message();
                            udpClient.Send(e403, e403.Length, destEp);
                        }
                        // analog out
                        {
                            IPEndPoint destEp = Config2.Instance.UeiCubes[0].DeviceSetupList[0].LocalEndPoint.ToIpEp();
                            byte[] e308 = StaticMethods.Make_A308Down_message();
                            udpClient.Send(e308, e308.Length, destEp);
                        }
#if dontremove
                        byte[] e430 = StaticMethods.Make_DIO430Down_Message();
                        udpClient.Send(e430, e308.Length, destEp);
#endif


                        // serial out
                        List<byte[]> e508 = StaticMethods.Make_SL508Down_Messages(i);
                        foreach (byte[] msg in e508)
                        {
                            IPEndPoint destEp = Config2.Instance.UeiCubes[0].DeviceSetupList[3].LocalEndPoint.ToIpEp();
                            udpClient.Send(msg, msg.Length, destEp);
                            System.Threading.Thread.Sleep(10);
                        }

                        // relays
                        {
                            //byte[] e470 = StaticMethods.Make_DIO470_Down_Message();
                            //IPEndPoint destEp = Config2.Instance.UeiCubes[0].DeviceSetupList[4].LocalEndPoint.ToIpEp();
                            //udpClient.Send(e470, e470.Length, destEp);
                        }

                        Thread.Sleep(100);

                    }
                    _logger.Info("Downward message simulation end");

                }
                catch (Exception ex)
                {
                    _logger.Warn(ex.Message);
                }
            });
        }

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
    }
}
