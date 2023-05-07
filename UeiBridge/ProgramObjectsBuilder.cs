#define blocksim1
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
    public class ProgramObjectsBuilder : IDisposable
    {
        ILog _logger = StaticMethods.GetLogger();
        List<PerDeviceObjects> _PerDeviceObjectsList;
        public List<PerDeviceObjects> PerDeviceObjectsList => _PerDeviceObjectsList;
        Config2 _mainConfig;

        public ProgramObjectsBuilder(Config2 mainConfig)
        {
            _mainConfig = mainConfig;
        }
        public DeviceType GetOutputDeviceManager<DeviceType>(string cubeUrl, int deviceSlot, string deviceName = null) where DeviceType : OutputDevice
        {
            var deviceInSlot = _PerDeviceObjectsList.Where(d => (d.OutputDeviceManager != null) && (d.SlotNumber == deviceSlot) && (d.CubeUrl == cubeUrl));
            OutputDevice od = (OutputDevice)deviceInSlot.Select(d => d.OutputDeviceManager).FirstOrDefault();

            // check device name 
            if ((null!=od)&& (deviceName != null) && (od.DeviceName != deviceName))
            {
                return null;
            }

            return od as DeviceType;
        }
        public DeviceType GetInputDeviceManager<DeviceType>(string cubeUrl, int deviceSlot, string deviceName = null) where DeviceType : InputDevice
        {
            var deviceInSlot = _PerDeviceObjectsList.Where(d => (d.InputDeviceManager != null) && (d.SlotNumber == deviceSlot) && (d.CubeUrl == cubeUrl));
            InputDevice id = (InputDevice)deviceInSlot.Select(d => d.InputDeviceManager).FirstOrDefault();

            // check device name 
            if ((null!=id)&(deviceName != null) && (id.DeviceName != deviceName))
            {
                return null;
            }

            return id as DeviceType;
        }

        public void CreateDeviceManagers(List<DeviceEx> realDeviceList)
        {
            if (realDeviceList == null)
            {
                return;
            }
            _PerDeviceObjectsList = new List<PerDeviceObjects>();

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
                DeviceSetup setup = _mainConfig.GetDeviceSetupEntry(realDevice.CubeUrl, realDevice.PhDevice.GetIndex());
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
                //setup.CubeUrl = realDevice.CubeUrl;
                //setup.IsBlockSensorActive = _mainConfig.Blocksensor.IsActive;
                List<PerDeviceObjects> objs = BuildObjectsForDevice(realDevice, setup);
                if (null != objs)
                {
                    _PerDeviceObjectsList.AddRange(objs);
                }
            }
        }

        public void ActivateDownstreamOjects()
        {
            // activate downward (output) objects
            foreach (PerDeviceObjects deviceObjects in _PerDeviceObjectsList)
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
            foreach (PerDeviceObjects deviceObjects in _PerDeviceObjectsList)
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
                case DeviceMap2.SimuAO16Literal:
                    {
                        return Build_SimuAO16_2(realDevice, setup);
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
            // if block-sensor is active, Do not build AO308,
            // since Block sensor takes control on the analog output. 
            BlockSensorSetup bssetup = _mainConfig.GetDeviceSetupEntry(setup.CubeUrl, BlockSensorSetup.BlockSensorSlotNumber) as BlockSensorSetup;
            bool bsActive = ((null != bssetup) && (true == bssetup.IsActive) && (bssetup.AnalogCardSlot == setup.SlotNumber)) ? true : false;
            if (bsActive)
            {
                return null;
            }

            // create uei entities
            Session theSession = new Session();
            string cubeUrl = $"{setup.CubeUrl}Dev{ setup.SlotNumber}/Ao0:7";
            var c = theSession.CreateAOChannel(cubeUrl, -AO308Setup.PeekVoltage_downstream, AO308Setup.PeekVoltage_downstream);
            System.Diagnostics.Debug.Assert(c.GetMaximum() == AO308Setup.PeekVoltage_downstream);
            theSession.ConfigureTimingForSimpleIO();
            var aWriter = new AnalogWriteAdapter(new AnalogScaledWriter(theSession.GetDataStream()), theSession);

            AO308OutputDeviceManager ao308 = new AO308OutputDeviceManager(setup as AO308Setup, aWriter, theSession, bsActive);
            PerDeviceObjects pd = new PerDeviceObjects(realDevice);

            // if block sensor isn't active, set ao308 as consumer of udp-reader
            //if (!ao308.IsBlockSensorActive)
            {
                var nic = IPAddress.Parse(_mainConfig.AppSetup.SelectedNicForMCast);
                UdpReader ureader = new UdpReader(setup.LocalEndPoint.ToIpEp(), nic, ao308, ao308.InstanceName);
                pd.UdpReader = ureader;
            }

            pd.OutputDeviceManager = ao308;

            return new List<PerDeviceObjects>() { pd };
        }
        List<PerDeviceObjects> Build_SimuAO16_2(DeviceEx realDevice, DeviceSetup setup)
        {
            Session theSession = new Session();
            string cubeUrl = $"{setup.CubeUrl}Dev{ setup.SlotNumber}/Ao0:7";
            var c = theSession.CreateAOChannel(cubeUrl, -AO308Setup.PeekVoltage_downstream, AO308Setup.PeekVoltage_downstream);
            System.Diagnostics.Debug.Assert(c.GetMaximum() == AO308Setup.PeekVoltage_downstream);
            theSession.ConfigureTimingForSimpleIO();
            var aWriter = new AnalogWriteAdapter(new AnalogScaledWriter(theSession.GetDataStream()), theSession);
            System.Diagnostics.Debug.Assert(null != (setup as SimuAO16Setup));
            SimuAO16OutputDeviceManager ao16 = new SimuAO16OutputDeviceManager(setup, aWriter, theSession);
            PerDeviceObjects pd = new PerDeviceObjects(realDevice);

            // set ao308 as consumer of udp-reader
            //if (_mainConfig.Blocksensor.IsActive == false)
            //{
            //    var nic = IPAddress.Parse(_mainConfig.AppSetup.SelectedNicForMCast);
            //    UdpReader ureader = new UdpReader(setup.LocalEndPoint.ToIpEp(), nic, ao16, ao16.InstanceName);
            //    pd.UdpReader = ureader;
            //}
            pd.OutputDeviceManager = ao16;

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
            DigitalWriterAdapter aWriter = new DigitalWriterAdapter(new UeiDaq.DigitalWriter(theSession.GetDataStream()));

            // output
            DIO403OutputDeviceManager od = new DIO403OutputDeviceManager(setup, aWriter, theSession);
            var nic = IPAddress.Parse(_mainConfig.AppSetup.SelectedNicForMCast);
            UdpReader ureader = new UdpReader(setup.LocalEndPoint.ToIpEp(), nic, od, od.InstanceName);

            // input
            string instanceName = $"{realDevice.PhDevice.GetDeviceName()}/Slot{realDevice.PhDevice.GetIndex()}";
            UdpWriter udpWriter = new UdpWriter(instanceName, setup.DestEndPoint.ToIpEp(), _mainConfig.AppSetup.SelectedNicForMCast);

            DIO403InputDeviceManager id = new DIO403InputDeviceManager(udpWriter, setup);

            PerDeviceObjects pd = new PerDeviceObjects(realDevice);
            pd.OutputDeviceManager = od;
            pd.UdpReader = ureader;
            pd.InputDeviceManager = id;
            pd.UdpWriter = udpWriter;
            return new List<PerDeviceObjects>() { pd };
        }

        public void Build_BlockSensorManager(List<DeviceEx> realDeviceList)
        {
            foreach (CubeSetup csetup in _mainConfig.CubeSetupList)
            {
                // check if block sensor is active for cube
                BlockSensorSetup bssetup = _mainConfig.GetDeviceSetupEntry(csetup.CubeUrl, BlockSensorSetup.BlockSensorSlotNumber) as BlockSensorSetup;
                if ((null == bssetup) || (false == bssetup.IsActive))
                {
                    continue;
                }

                Session theSession = new Session();
                string cubeUrl = $"{ csetup.CubeUrl}Dev{ bssetup.AnalogCardSlot}/Ao0:7";
                var ch = theSession.CreateAOChannel(cubeUrl, -AO308Setup.PeekVoltage_downstream, AO308Setup.PeekVoltage_downstream);
                System.Diagnostics.Debug.Assert(ch.GetMaximum() == AO308Setup.PeekVoltage_downstream);
                theSession.ConfigureTimingForSimpleIO();
                var aWriter = new AnalogWriteAdapter(new AnalogScaledWriter(theSession.GetDataStream()), theSession);

                BlockSensorManager2 blockSensor = new BlockSensorManager2(bssetup, aWriter, theSession);

#if !blocksim
                // redirect dio430/input to block-sensor.
                IEnumerable<PerDeviceObjects> inputDevices = _PerDeviceObjectsList.Where(i => i.InputDeviceManager != null).Where(i => i.InputDeviceManager.DeviceName == DeviceMap2.DIO403Literal).Where(i => i.InputDeviceManager.SlotNumber == bssetup.DigitalCardSlot);
                DIO403InputDeviceManager dio403 = inputDevices.Select(i => i.InputDeviceManager).FirstOrDefault() as DIO403InputDeviceManager;
                System.Diagnostics.Debug.Assert(dio403 != null);
                dio403.TargetConsumer = blockSensor;
#endif
                // define udp-reader for block sensor
                var nic = IPAddress.Parse(_mainConfig.AppSetup.SelectedNicForMCast);
                UdpReader ureader = new UdpReader(bssetup.LocalEndPoint.ToIpEp(), nic, blockSensor, "BlockSensor");

                // add block sensor to device list
                PerDeviceObjects pd = new PerDeviceObjects(blockSensor.DeviceName, -1, "no_cube");
                pd.OutputDeviceManager = blockSensor;
                pd.UdpReader = ureader;

                blockSensor.OpenDevice();
                ureader.Start();

#if blocksim
                byte[] d403 = Library.StaticMethods.Make_Dio403_upstream_message(new byte[] { 0x5, 0, 0 });
                blockSensor.Enqueue(d403);
#endif
                _PerDeviceObjectsList.Add(pd);

            }


        }

        public void Dispose()
        {
            foreach (var entry in _PerDeviceObjectsList)
            {
                entry.UdpReader?.Dispose();
                System.Threading.Thread.Sleep(50);
                entry.OutputDeviceManager?.Dispose();
                System.Threading.Thread.Sleep(50);

                entry.InputDeviceManager?.Dispose();
                System.Threading.Thread.Sleep(50);
                entry.UdpWriter?.Dispose();

            }
        }
    }
}
