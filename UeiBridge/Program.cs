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
using UeiBridgeTypes;

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

            // prepare device dictionaries
            ProjectRegistry.Instance.Establish();

            BuildProgramObjects();

            // publish status to StatusViewer
            Task.Factory.StartNew(() => PublishStatus_Task());

            // self tests
            StartDownwardsTest();

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
                        dl[slot]._outputDeviceManager.Dispose();
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
            foreach (var cube in Config2.Instance.UeiCubes)
            {
                // init outputDeviceList
                List<UeiDaq.Device> realDeviceList = StaticMethods.GetDeviceList(cube.CubeUrl);
                _deviceObjectsTable[cube.CubeNumber] = new List<PerDeviceObjects>(new PerDeviceObjects[realDeviceList.Count]);

                CreateDownwardsObjects(cube, _deviceObjectsTable);
                CreateUpwardsObjects(cube, _deviceObjectsTable);
            }

            // Activate program Objects
            foreach (List<PerDeviceObjects> cube in _deviceObjectsTable)
            {
                ActivateProgramObjects(cube);// _deviceObjectsTable);
            }

        }


        private static void ActivateProgramObjects(List<PerDeviceObjects> cube)
        {
            // activate downward (output) objects
            foreach (PerDeviceObjects deviceObjects in cube)
            {
                Thread.Sleep(100);
                deviceObjects?._outputDeviceManager?.OpenDevice();
                Thread.Sleep(100);
                deviceObjects?._udpReader?.Start();
            }

            // activate upward (input) objects
            foreach (PerDeviceObjects deviceObjects in cube)
            {
                Thread.Sleep(100);
                deviceObjects?._inputDeviceManager?.OpenDevice();
                //Thread.Sleep(100);
                //deviceObjects?._udpWriter?.Start();
            }
        }

        void DisposeProgramObjects()
        {
            for (int cubeIndex = 0; cubeIndex < _deviceObjectsTable.Count; cubeIndex++)
            {
                List<PerDeviceObjects> devList = _deviceObjectsTable[cubeIndex];
                for (int deviceIndex = 0; deviceIndex < devList.Count; deviceIndex++)
                {
                    devList[deviceIndex]?._udpReader?.Dispose();
                    devList[deviceIndex]?._outputDeviceManager?.Dispose();
                }
            }
        }

        /// <summary>
        /// Create output device managers and udp readers
        /// </summary>
        private static void CreateDownwardsObjects(CubeSetup cubeSetup, List<List<PerDeviceObjects>> deviceObjectsTable)
        {
            List<UeiDaq.Device> realDeviceList = StaticMethods.GetDeviceList(cubeSetup.CubeUrl);

            // populate outputDeviceList
            foreach (UeiDaq.Device realDevice in realDeviceList)
            {
                int realSlot = realDevice.GetIndex();
                DeviceSetup deviceSetup = Config2.Instance.UeiCubes[cubeSetup.CubeNumber].DeviceSetupList[realSlot]; // tbd: first 'DeviceSetup' in config is not neccesseraly in slot 0
                deviceSetup.CubeUrl = cubeSetup.CubeUrl;
                System.Diagnostics.Debug.Assert(realSlot == deviceSetup.SlotNumber);
                System.Diagnostics.Debug.Assert(realDevice.GetDeviceName() == deviceSetup.DeviceName);

                Type devType = StaticMethods.GetDeviceManagerType<OutputDevice>(deviceSetup.DeviceName);
                if (null == devType)
                    continue;

                Session serialSession = null;
                OutputDevice outDev;
                if (devType.Name.StartsWith("SL508")) // special treatment to serial device
                {
                    if (null == deviceObjectsTable[cubeSetup.CubeNumber][realSlot])
                    {
                        serialSession = StaticMethods.CreateSerialSession(deviceSetup as SL508892Setup);
                    }
                    else
                    { 
                        serialSession = deviceObjectsTable[cubeSetup.CubeNumber][realSlot].SerialSession; 
                    }
                    outDev = (OutputDevice)Activator.CreateInstance(devType, deviceSetup, serialSession);
                }
                else
                {
                    outDev = (OutputDevice)Activator.CreateInstance(devType, deviceSetup);
                }

                System.Diagnostics.Debug.Assert(null != outDev);
                System.Diagnostics.Debug.Assert(null != deviceSetup.LocalEndPoint);

                UdpReader ureader = new UdpReader(deviceSetup.LocalEndPoint.ToIpEp(), outDev, outDev.InstanceName);
                if (null == deviceObjectsTable[cubeSetup.CubeNumber][realSlot])
                {
                    deviceObjectsTable[cubeSetup.CubeNumber][realSlot] = new PerDeviceObjects(outDev, ureader);
                    deviceObjectsTable[cubeSetup.CubeNumber][realSlot].SerialSession = serialSession;
                }
                else
                {
                    deviceObjectsTable[cubeSetup.CubeNumber][realSlot].NewObjects(outDev, ureader);
                }
            }
        }

        /// <summary>
        /// Creaete input-device managers and udp-writers
        /// </summary>
        /// <param name="cubeSetup"></param>
        /// <param name="deviceObjectsTable"></param>
        private static void CreateUpwardsObjects(CubeSetup cubeSetup, List<List<PerDeviceObjects>> deviceObjectsTable)
        {
            List<UeiDaq.Device> realDeviceList = StaticMethods.GetDeviceList(cubeSetup.CubeUrl);

            // Create input-devices instances
            foreach (UeiDaq.Device realDevice in realDeviceList)
            {
                int realSlot = realDevice.GetIndex();
                DeviceSetup deviceSetup = Config2.Instance.UeiCubes[cubeSetup.CubeNumber].DeviceSetupList[realSlot]; // tbd: first 'DeviceSetup' in config is not neccesseraly in slot 0
                deviceSetup.CubeUrl = cubeSetup.CubeUrl; // for later use
                System.Diagnostics.Debug.Assert(realSlot == deviceSetup.SlotNumber);
                System.Diagnostics.Debug.Assert(realDevice.GetDeviceName() == deviceSetup.DeviceName);

                Type devType = StaticMethods.GetDeviceManagerType<InputDevice>(deviceSetup.DeviceName);
                if (null == devType)
                    continue;

                string instanceName = $"{realDevice.GetDeviceName()}/Slot{deviceSetup.SlotNumber}";
                UdpWriter uWriter = new UdpWriter(instanceName, null);
                InputDevice inDev;
                if (devType.Name.StartsWith("SL508"))
                {
                    inDev = (InputDevice)Activator.CreateInstance(devType, uWriter, deviceSetup, deviceObjectsTable[cubeSetup.CubeNumber][realSlot].SerialSession);
                }
                else
                {
                    inDev = (InputDevice)Activator.CreateInstance(devType, uWriter, deviceSetup);
                }

                if (null == deviceObjectsTable[cubeSetup.CubeNumber][realSlot])
                {
                    deviceObjectsTable[cubeSetup.CubeNumber][realSlot] = new PerDeviceObjects(inDev, uWriter);
                }
                else
                {
                    deviceObjectsTable[cubeSetup.CubeNumber][realSlot].NewObjects(inDev, uWriter);
                }

                //if (null != inDev)
                //{
                //    System.Diagnostics.Debug.Assert(null != deviceSetup.LocalEndPoint);
                //    UdpWriter ur = new UdpReader(deviceSetup.LocalEndPoint.ToIpEp(), inDev, inDev.InstanceName);
                //    deviceObjectsTable[cubeSetup.CubeNumber][realSlot] = new PerDeviceObjects(inDev, ur);
                //}
                //else
                //{
                //    //_logger.Warn($"null device {deviceSetup.DeviceName}");
                //    deviceObjectsTable[cubeSetup.CubeNumber][realSlot] = null;
                //}
            }
        }


        void PublishStatus_Task()
        {
            UdpWriter uw = new UdpWriter("to-statusViewer", Config.Instance.SelectedNicForMcastSend);

            while (true)
            {
                foreach (PerDeviceObjects deviceObjects in _deviceObjectsTable[0]) //ProjectRegistry.Instance.OutputDevicesMap)
                {
                    // output
                    if (null != deviceObjects?._outputDeviceManager)
                    {
                        UeiLibrary.JsonStatusClass js = new UeiLibrary.JsonStatusClass(deviceObjects._outputDeviceManager.DeviceName + " (Output)", deviceObjects._outputDeviceManager.GetFormattedStatus());
                        string s = Newtonsoft.Json.JsonConvert.SerializeObject(js);
                        byte[] send_buffer = Encoding.ASCII.GetBytes(s);
                        uw.Send(send_buffer);
                    }
                }

                //foreach (var item in _inputDevices)
                //{
                //    UeiLibrary.JsonStatusClass js = new UeiLibrary.JsonStatusClass(item.DeviceName + " (Input)", item.GetFormattedStatus());
                //    string s = Newtonsoft.Json.JsonConvert.SerializeObject(js);
                //    byte[] send_buffer = Encoding.ASCII.GetBytes(s);
                //    uw.Send(send_buffer);
                //}

                System.Threading.Thread.Sleep(1000);
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
                    IPEndPoint destEp = new IPEndPoint(IPAddress.Parse(Config.Instance.ReceiverMulticastAddress), Config.Instance.ReceiverMulticastPort);

                    for (int i = 0; i < 1; i++)
                    {

                        // digital out
                        {
                            destEp = Config2.Instance.UeiCubes[0].DeviceSetupList[5].LocalEndPoint.ToIpEp();
                            byte[] e403 = StaticMethods.Make_DIO403Down_Message();
                            //udpClient.Send(e403, e403.Length, destEp);
                        }
                        // analog out
                        {
                            destEp = Config2.Instance.UeiCubes[0].DeviceSetupList[0].LocalEndPoint.ToIpEp();
                            byte[] e308 = StaticMethods.Make_A308Down_message();
                            //udpClient.Send(e308, e308.Length, destEp);
                        }
#if dontremove
                        byte[] e430 = StaticMethods.Make_DIO430Down_Message();
                        udpClient.Send(e430, e308.Length, destEp);
#endif
                        // serial out
                        List<byte[]> e508 = StaticMethods.Make_SL508Down_Messages( i);
                        foreach (byte [] msg in e508)
                        {
                            destEp = Config2.Instance.UeiCubes[0].DeviceSetupList[3].LocalEndPoint.ToIpEp();
                            udpClient.Send(msg, msg.Length, destEp);
                            System.Threading.Thread.Sleep(200);
                        }


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
