﻿#define blocksim
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using UeiDaq;
using log4net;
using UeiBridge.Library;
using UeiBridge.Types;

namespace UeiBridge
{
    public class ProgramObjectsBuilder
    {
        ILog _logger = StaticMethods.GetLogger();
        List<PerDeviceObjects> _deviceManagers;
        public List<PerDeviceObjects> DeviceManagers => _deviceManagers;
        Config2 _mainConfig;

        public ProgramObjectsBuilder(Config2 mainConfig)
        {
            _mainConfig = mainConfig;
        }

        public void CreateDeviceManagers(List<DeviceEx> realDeviceList)
        {
            if (realDeviceList == null)
            {
                return;
            }
            _deviceManagers = new List<PerDeviceObjects>();

            foreach (DeviceEx realDevice in realDeviceList)
            {
                // prologue
                // =========
                // it type exists
                var t = StaticMethods.GetDeviceManagerType<IDeviceManager>(realDevice.PhDevice.GetDeviceName());
                if (null == t)
                {
                    _logger.Debug($"Device {realDevice.PhDevice.GetDeviceName()} not supported");
                    continue;
                }
                // if config entry exists
                DeviceSetup setup = _mainConfig.GetSetupEntryForDevice(realDevice.CubeUrl, realDevice.PhDevice.GetIndex());
                if (null == setup)
                {
                    _logger.Warn($"No config entry for {realDevice.CubeUrl},  {realDevice.PhDevice.GetDeviceName()}, Slot {realDevice.PhDevice.GetIndex()}");
                    continue;
                }
                // if config entry match
                if (setup.DeviceName != realDevice.PhDevice.GetDeviceName())
                {
                    _logger.Warn($"Config entry at slot {realDevice.PhDevice.GetIndex()}/ Cube {realDevice.CubeUrl} does not match physical device {realDevice.PhDevice.GetDeviceName()}");
                    continue;
                }

                // build manager(s)
                setup.CubeUrl = realDevice.CubeUrl;
                setup.IsBlockSensorActive = _mainConfig.Blocksensor.IsActive;
                List<PerDeviceObjects> objs = BuildObjectsForDevice(realDevice, setup);
                _deviceManagers.AddRange(objs);
            }
        }

        public void ActivateDownstreamOjects()
        {
            // activate downward (output) objects
            foreach (PerDeviceObjects deviceObjects in _deviceManagers)
            {
                if (null != deviceObjects)
                {
                    if (null != deviceObjects.OutputDeviceManager)
                    {
                        deviceObjects.OutputDeviceManager.OpenDevice();
                        deviceObjects.UdpReader?.Start();
                    }
                    System.Threading.Thread.Sleep(10);
                }
            }
        }

        public void ActivateUpstreamObjects()
        {
            // activate upward (input) objects
            foreach (PerDeviceObjects deviceObjects in _deviceManagers)
            {
                deviceObjects?.InputDeviceManager?.OpenDevice();
                System.Threading.Thread.Sleep(10);
                // (no need to activate udpWriter)
            }
        }

        private List<PerDeviceObjects> BuildObjectsForDevice(DeviceEx realDevice, DeviceSetup setup)
        {
            switch (realDevice.PhDevice.GetDeviceName())
            {
                case "Simu-AO16":
                    {
                        return Build_SimuAO16(realDevice, setup);
                    }
                case DeviceMap2.AO308Literal:
                    {
                        return Build_AO308(realDevice, setup);
                    }
                case DeviceMap2.DIO403Literal:
                    {
                        return Build_DIO403(realDevice, setup);
                    }
                case DeviceMap2.DIO470Literal:
                    {
                        return Build_DIO470(realDevice, setup);
                    }
                case DeviceMap2.AI201Literal:
                    {
                        return Build_AI201(realDevice, setup);
                    }
                case DeviceMap2.SL508Literal:
                    {
                        return Build_SL508(realDevice, setup);
                    }
                default:
                    {
                        _logger.Warn($"Failed to build {realDevice.PhDevice.GetDeviceName()}");
                        return new List<PerDeviceObjects>(); // return empty
                    }
            }
        }

        private List<PerDeviceObjects> Build_AO308(DeviceEx realDevice, DeviceSetup setup)
        {
            //AnalogWriteAdapter analogWriter = new AnalogWriteAdapter();

            Session theSession = new Session();
            string cubeUrl = $"{setup.CubeUrl}Dev{ setup.SlotNumber}/Ao0:7";
            var c = theSession.CreateAOChannel(cubeUrl, -AO308Setup.PeekVoltage_downstream, AO308Setup.PeekVoltage_downstream);
            System.Diagnostics.Debug.Assert(c.GetMaximum() == AO308Setup.PeekVoltage_downstream);
            theSession.ConfigureTimingForSimpleIO();
            var aWriter = new AnalogWriteAdapter(new AnalogScaledWriter(theSession.GetDataStream()), theSession);

            AO308OutputDeviceManager ao308 = new AO308OutputDeviceManager(setup as AO308Setup, aWriter);
            PerDeviceObjects pd = new PerDeviceObjects(realDevice);

            // set ao308 as consumer of udp-reader
            if (_mainConfig.Blocksensor.IsActive == false)
            {
                var nic = IPAddress.Parse(_mainConfig.AppSetup.SelectedNicForMCast);
                UdpReader ureader = new UdpReader(setup.LocalEndPoint.ToIpEp(), nic, ao308, ao308.InstanceName);
                pd.UdpReader = ureader;
            }
            pd.OutputDeviceManager = ao308;

            return new List<PerDeviceObjects>() { pd };
        }

        private List<PerDeviceObjects> Build_SL508(DeviceEx realDevice, DeviceSetup setup)
        {
            SL508Session serialSession = new SL508Session(setup as SL508892Setup);//, realDevice.CubeUrl);
            System.Diagnostics.Debug.Assert(null != serialSession);

            string instanceName = $"{realDevice.PhDevice.GetDeviceName()}/Slot{realDevice.PhDevice.GetIndex()}";
            UdpWriter uWriter = new UdpWriter(instanceName, setup.DestEndPoint.ToIpEp(), _mainConfig.AppSetup.SelectedNicForMCast);
            SL508InputDeviceManager id = new SL508InputDeviceManager(uWriter, setup, serialSession);

            SL508OutputDeviceManager od = new SL508OutputDeviceManager(setup, serialSession);
            var nic = IPAddress.Parse(_mainConfig.AppSetup.SelectedNicForMCast);
            UdpReader ureader = new UdpReader(setup.LocalEndPoint.ToIpEp(), nic, od, od.InstanceName);

            var pd = new PerDeviceObjects(realDevice);
            pd.SerialSession = serialSession;
            pd.InputDeviceManager = id;
            pd.OutputDeviceManager = od;
            pd.UdpReader = ureader;
            pd.UdpWriter = uWriter;

            return new List<PerDeviceObjects>() { pd };
        }

        private List<PerDeviceObjects> Build_AI201(DeviceEx realDevice, DeviceSetup setup)
        {
            string instanceName = $"{realDevice.PhDevice.GetDeviceName()}/Slot{realDevice.PhDevice.GetIndex()}";
            UdpWriter uWriter = new UdpWriter(instanceName, setup.DestEndPoint.ToIpEp(), _mainConfig.AppSetup.SelectedNicForMCast);
            AI201InputDeviceManager id = new AI201InputDeviceManager(uWriter, setup as AI201100Setup);

            var pd = new PerDeviceObjects(realDevice);
            pd.UdpWriter = uWriter;
            pd.InputDeviceManager = id;

            return new List<PerDeviceObjects>() { pd };
        }

        private List<PerDeviceObjects> Build_DIO470(DeviceEx realDevice, DeviceSetup setup)
        {
            DIO470OutputDeviceManager od = new DIO470OutputDeviceManager(setup);
            var nic = IPAddress.Parse(_mainConfig.AppSetup.SelectedNicForMCast);
            UdpReader ureader = new UdpReader(setup.LocalEndPoint.ToIpEp(), nic, od, od.InstanceName);

            PerDeviceObjects pd = new PerDeviceObjects(realDevice);
            pd.OutputDeviceManager = od;
            pd.UdpReader = ureader;
            return new List<PerDeviceObjects>() { pd };
        }

        private List<PerDeviceObjects> Build_DIO403(DeviceEx realDevice, DeviceSetup setup)
        {
            string cubeUrl = $"{setup.CubeUrl}Dev{ setup.SlotNumber}/Do0:2";// Do0:2 - 3*8 first bits as 'out'
            Session theSession = new UeiDaq.Session();
            theSession.CreateDOChannel(cubeUrl);
            theSession.ConfigureTimingForSimpleIO();
            DigitalWriterAdapter aWriter = new DigitalWriterAdapter(new UeiDaq.DigitalWriter(theSession.GetDataStream()), theSession);

            // output
            DIO403OutputDeviceManager od = new DIO403OutputDeviceManager(setup, aWriter);
            var nic = IPAddress.Parse(_mainConfig.AppSetup.SelectedNicForMCast);
            UdpReader ureader = new UdpReader(setup.LocalEndPoint.ToIpEp(), nic, od, od.InstanceName);

            // input
            string instanceName = $"{realDevice.PhDevice.GetDeviceName()}/Slot{realDevice.PhDevice.GetIndex()}";
            UdpWriter uWriter = new UdpWriter(instanceName, setup.DestEndPoint.ToIpEp(), _mainConfig.AppSetup.SelectedNicForMCast);

            DIO403InputDeviceManager id = new DIO403InputDeviceManager(uWriter, setup);

            PerDeviceObjects pd = new PerDeviceObjects(realDevice);
            pd.OutputDeviceManager = od;
            pd.UdpReader = ureader;
            pd.InputDeviceManager = id;
            pd.UdpWriter = uWriter;
            return new List<PerDeviceObjects>() { pd };
        }

        List<PerDeviceObjects> Build_SimuAO16(DeviceEx realDevice, DeviceSetup setup)
        {
            //List<PerDeviceObjects> result = new List<PerDeviceObjects>();

            DeviceSetup deviceSetup = setup;
            Type devType = StaticMethods.GetDeviceManagerType<IDeviceManager>(realDevice.PhDevice.GetDeviceName());
            OutputDevice outDev = (OutputDevice)Activator.CreateInstance(devType, deviceSetup);

            System.Diagnostics.Debug.Assert(null != outDev);
            System.Diagnostics.Debug.Assert(null != deviceSetup.LocalEndPoint);

            // create udp reader for this device
            var nic = IPAddress.Parse(_mainConfig.AppSetup.SelectedNicForMCast);
            UdpReader ureader = new UdpReader(deviceSetup.LocalEndPoint.ToIpEp(), nic, outDev, outDev.InstanceName);

            var pdo = new PerDeviceObjects(realDevice);
            pdo.OutputDeviceManager = outDev;
            pdo.UdpReader = ureader;

            return new List<PerDeviceObjects>() { pdo };
        }

        public void CreateBlockSensorManager(List<DeviceEx> realDeviceList)
        {
            BlockSensorManager blockSensor = CreateBlockSensorObject(realDeviceList);
            if (null != blockSensor)
            {
#if !blocksim
                // redirect dio430/input to blocksensor
                var outDevs = _deviceManagers.Where(i => i.InputDeviceManager != null);
                var dio = outDevs.Where(i => i.InputDeviceManager.DeviceName.StartsWith("DIO-4")).Select(i => i.InputDeviceManager).FirstOrDefault();
                DIO403InputDeviceManager di403 = dio as DIO403InputDeviceManager;
                di403.TargetConsumer = blockSensor;
#endif
                // define udp-reader for block sensor
                var nic = IPAddress.Parse(_mainConfig.AppSetup.SelectedNicForMCast);
                UdpReader ureader = new UdpReader(_mainConfig.Blocksensor.LocalEndPoint.ToIpEp(), nic, blockSensor, "BlockSensor");

                // add block sensor to device list
                PerDeviceObjects pd = new PerDeviceObjects(blockSensor.DeviceName, -1, "no_cube");

                blockSensor.OpenDevice();
                ureader.Start();

#if blocksim
                byte[] d403 = Library.StaticMethods.Make_Dio403_upstream_message();
                blockSensor.Enqueue(d403);
#endif
                _deviceManagers.Add(pd);
            }

        }

        /// <summary>
        /// BlockSensorManager might be created only if digital and analog cards exists
        /// </summary>
        private BlockSensorManager CreateBlockSensorObject(List<DeviceEx> realDeviceList)
        {
            if (false == _mainConfig.Blocksensor.IsActive)
            {
                _logger.Debug("Block sensor disabled.");
                return null;
            }
            BlockSensorManager result = null;
            var outDevs = _deviceManagers.Where(d => (d.OutputDeviceManager != null) && d.OutputDeviceManager.DeviceName.StartsWith("AO-308")).Select(d => d.OutputDeviceManager);
            if (outDevs.Count() > 0) // if there is a ao308 card
            {
                AO308OutputDeviceManager ao308 = outDevs.FirstOrDefault() as AO308OutputDeviceManager;
                bool digitalExist = realDeviceList.Any(d => d.PhDevice.GetDeviceName().StartsWith("DIO-403"));
                if (digitalExist && ao308 != null)
                {
                    result = new BlockSensorManager(_mainConfig.Blocksensor, ao308.AnalogWriter);
                }
                else
                {
                    _logger.Warn("Failed to create blocksensor object");
                }
            }
            return result;
        }

    }
}
