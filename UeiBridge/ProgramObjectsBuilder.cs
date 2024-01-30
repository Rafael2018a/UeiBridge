//#define blocksim // block sensor simulation
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using UeiDaq;
using log4net;
using UeiBridge.Library;
using UeiBridge.Library.Types;
using System.Collections.Concurrent;
using UeiBridge.Library.CubeSetupTypes;
using UeiBridge.Library.Interfaces;

namespace UeiBridge
{
    public class ProgramObjectsBuilder : IDisposable
    {
        ILog _logger = StaticLocalMethods.GetLogger();
        List<PerDeviceObjects> _PerDeviceObjectsList;
        List<IDeviceManager> _deviceManagerList; // for the time being (jan24) this is just for the new serial device handler
        List<UdpReader> _udpReaderList;
        public List<PerDeviceObjects> PerDeviceObjectsList => _PerDeviceObjectsList;
        public List<IDeviceManager> DeviceManagerList => _deviceManagerList;
        public List<UdpReader> UdpReadersList => _udpReaderList;


        UdpToSlotMessenger _udpMessenger = new UdpToSlotMessenger();
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
            if ((null != od) && (deviceName != null) && (od.DeviceName != deviceName))
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
            if ((null != id) & (deviceName != null) && (id.DeviceName != deviceName))
            {
                return null;
            }

            return id as DeviceType;
        }

        protected void EmitInitMessage(UeiDeviceInfo deviceInfo, string deviceMessage)
        {
            _logger.Info($"Cube{deviceInfo.CubeId}/Slot{deviceInfo.DeviceSlot}: {deviceMessage}");
        }

        /// <summary>
        /// For each device in slot, build a device manager and add it to _PerDeviceObjectsList
        /// </summary>
        public void CreateDeviceManagers(List<UeiDeviceInfo> deviceInfoList)
        {
            if (deviceInfoList == null)
            {
                return;
            }
            _PerDeviceObjectsList = new List<PerDeviceObjects>();
            _deviceManagerList = new List<IDeviceManager>();
            
            _udpReaderList = new List<UdpReader>();

            foreach (UeiDeviceInfo deviceInfo in deviceInfoList)
            {
                string deviceMessage = null;
                // prologue
                // =========
                // it type exists
                Type t = StaticLocalMethods.GetDeviceManagerType<IDeviceManager>(deviceInfo.DeviceName);
                if (null == t)
                {
                    deviceMessage = $"Device {deviceInfo.DeviceName} not supported by software";
                    EmitInitMessage(deviceInfo, deviceMessage);
                    continue;
                }
                // if config entry exists
                DeviceSetup setup = _mainConfig.GetDeviceSetupEntry(deviceInfo.CubeUrl, deviceInfo.DeviceSlot);
                if (null == setup)
                {
                    deviceMessage = $"No config entry for {deviceInfo.CubeUrl},  {deviceInfo.DeviceName}, Slot {deviceInfo.DeviceSlot}";
                    EmitInitMessage(deviceInfo, deviceMessage);
                    continue;
                }
                if (false == setup.IsEnabled)
                {
                    continue;
                }
                // if config entry match
                if (setup.DeviceName != deviceInfo.DeviceName)
                {
                    deviceMessage = $"Config entry at slot {deviceInfo.DeviceSlot}/ Cube {deviceInfo.CubeUrl} does not match physical device {deviceInfo.DeviceName}";
                    EmitInitMessage(deviceInfo, deviceMessage);
                    continue;
                }

                // build manager(s)
                //setup.CubeUrl = realDevice.CubeUrl;
                //setup.IsBlockSensorActive = _mainConfig.Blocksensor.IsActive;

                if (deviceInfo.DeviceName == DeviceMap2.SL508Literal)
                {
                    var dm = BuildDeviceManager(deviceInfo, setup);
                    if (null!=dm)
                    {
                        _deviceManagerList.Add(dm);
                    }
                }
                else
                {
                    List<PerDeviceObjects> objs = BuildObjectsForDevice(deviceInfo, setup);
                    if (null != objs)
                    {
                        _PerDeviceObjectsList.AddRange(objs);
                    }
                }


            }
        }

        public void ActivateDownstreamObjects()
        {
            // activate downward (output) objects
            foreach (PerDeviceObjects deviceObjects in _PerDeviceObjectsList)
            {
                if (null != deviceObjects)
                {
                    if (null != deviceObjects.OutputDeviceManager)
                    {
                        deviceObjects.OutputDeviceManager.OpenDevice();
                    }
                    System.Threading.Thread.Sleep(10);
                }
            }
            foreach (UdpReader ureader in _udpReaderList)
            {
                ureader.Start();
            }
        }

        public void ActivateUpstreamObjects()
        {
            // activate upward (input) objects
            foreach (PerDeviceObjects deviceObjects in _PerDeviceObjectsList)
            {
                _logger.Debug($"Activating {deviceObjects.DeviceName}");
                deviceObjects?.InputDeviceManager?.OpenDevice();
                System.Threading.Thread.Sleep(10);
                // (no need to activate udpWriter)
            }
        }

        IDeviceManager BuildSerialDeviceManager(UeiDeviceInfo realDevice, DeviceSetup setup)
        {
            IDeviceManager result=null;

            //switch (realDevice.DeviceName)
            //{
            //    case DeviceMap2.SL508Literal:
            //        _logger.Info("Building SL508...");
            //        SerialManagerTaskClass smtc = new SerialManagerTaskClass();
            //        smtc.MainSerial(realDevice, setup as SL508892Setup);
            //        result = smtc;
            //        break;
            //    default:
            //        _logger.Warn($"Failed to build {realDevice.DeviceName}");
            //        break;
            //}
            return result;
        }
        List<PerDeviceObjects> BuildObjectsForDevice(UeiDeviceInfo realDevice, DeviceSetup setup)
        {
            switch (realDevice.DeviceName)
            {
                //case DeviceMap2.SimuAO16Literal:
                //    {
                //        return Build_SimuAO16_2(realDevice, setup);
                //    }
                case DeviceMap2.AO308Literal:
                case DeviceMap2.AO322Literal:
                case DeviceMap2.SimuAO16Literal:
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
                        //Build_SL508_Failsafe(realDevice, setup);
                        return Build_SL508(realDevice, setup);

                    }
                case DeviceMap2.CAN503Literal:
                    {
                        return Build_CAN503(realDevice, setup);
                    }
                //case DeviceMap2.AO322Literal:
                //    {
                //        return Build_AO332(realDevice, setup);
                //    }
                default:
                    {
                        _logger.Warn($"Failed to build {realDevice.DeviceName}");
                        return new List<PerDeviceObjects>(); // return empty
                    }
            }
        }

        private List<PerDeviceObjects> Build_AO308(UeiDeviceInfo realDevice, DeviceSetup setup)
        {
            // if block-sensor is active, Do not build AO308,
            // since Block sensor takes control on the analog output. 
            BlockSensorSetup bssetup = _mainConfig.GetDeviceSetupEntry(setup.CubeUrl, realDevice.DeviceSlot) as BlockSensorSetup;
            bool bsActive = ((null != bssetup) && (true == bssetup.IsActive)) ? true : false;
            if (bsActive)
            {
                return null;
            }

            // create uei entities
            Session theSession = new Session();
            string cubeUrl = $"{setup.CubeUrl}Dev{setup.SlotNumber}/Ao0:7";
            var c = theSession.CreateAOChannel(cubeUrl, -AO308Setup.PeekVoltage_downstream, AO308Setup.PeekVoltage_downstream);
            System.Diagnostics.Debug.Assert(c.GetMaximum() == AO308Setup.PeekVoltage_downstream);
            theSession.ConfigureTimingForSimpleIO();
            theSession.Start();
            //var aWriter = new AnalogWriteAdapter(new AnalogScaledWriter(theSession.GetDataStream()));
            SessionAdapter tsa = new SessionAdapter(theSession);

            OutputDevice analogOut = null;
            if (realDevice.DeviceName == DeviceMap2.AO308Literal)
            {
                analogOut = new AO308OutputDeviceManager(setup as AO308Setup, tsa, bsActive);
            }
            if (realDevice.DeviceName == DeviceMap2.AO322Literal)
            {
                analogOut = new AO332OutputDeviceManager(setup as AO308Setup, tsa, bsActive);
            }
            if (realDevice.DeviceName == DeviceMap2.SimuAO16Literal)
            {
                analogOut = new SimuAO16OutputDeviceManager(setup as AO308Setup, tsa);
            }

            PerDeviceObjects pd = new PerDeviceObjects(realDevice);

            var nic = IPAddress.Parse(_mainConfig.AppSetup.SelectedNicForMulticast);
            UdpReader ureader = new UdpReader(setup.LocalEndPoint.ToIpEp(), nic, _udpMessenger, analogOut.InstanceName);
            _udpMessenger.SubscribeConsumer(analogOut, setup.GetCubeId(), setup.SlotNumber);
            _udpReaderList.Add(ureader);

            pd.OutputDeviceManager = analogOut;

            return new List<PerDeviceObjects>() { pd };
        }

        //private List<PerDeviceObjects> Build_AO332(UeiDeviceInfo realDevice, DeviceSetup setup)
        //{
        //    // create uei entities
        //    Session theSession = new Session();
        //    string cubeUrl = $"{setup.CubeUrl}Dev{setup.SlotNumber}/Ao0:31";
        //    var c = theSession.CreateAOChannel(cubeUrl, -AO308Setup.PeekVoltage_downstream, AO308Setup.PeekVoltage_downstream);
        //    System.Diagnostics.Debug.Assert(c.GetMaximum() == AO308Setup.PeekVoltage_downstream);
        //    theSession.ConfigureTimingForSimpleIO();
        //    theSession.Start();
        //    //var aWriter = new AnalogWriteAdapter(new AnalogScaledWriter(theSession.GetDataStream()));
        //    SessionAdapter tsa = new SessionAdapter(theSession);


        //    AO332OutputDeviceManager ao322 = new AO332OutputDeviceManager(setup as AO308Setup, tsa);
        //    PerDeviceObjects pd = new PerDeviceObjects(realDevice);

        //    var nic = IPAddress.Parse(_mainConfig.AppSetup.SelectedNicForMCast);
        //    UdpReader ureader = new UdpReader(setup.LocalEndPoint.ToIpEp(), nic, _udpMessenger, ao322.InstanceName);
        //    _udpMessenger.SubscribeConsumer(ao322, setup.CubeId, setup.SlotNumber);
        //    _udpReaderList.Add(ureader);

        //    pd.OutputDeviceManager = ao322;

        //    return new List<PerDeviceObjects>() { pd };
        //}


        //List<PerDeviceObjects> Build_SimuAO16_2(UeiDeviceInfo realDevice, DeviceSetup setup)
        //{
        //    Session theSession = new Session();
        //    string cubeUrl = $"{setup.CubeUrl}Dev{setup.SlotNumber}/Ao0:7";
        //    var c = theSession.CreateAOChannel(cubeUrl, -AO308Setup.PeekVoltage_downstream, AO308Setup.PeekVoltage_downstream);
        //    System.Diagnostics.Debug.Assert(c.GetMaximum() == AO308Setup.PeekVoltage_downstream);
        //    theSession.ConfigureTimingForSimpleIO();
        //    theSession.Start();
        //    //var aWriter = new AnalogWriteAdapter(new AnalogScaledWriter(theSession.GetDataStream()));
        //    //System.Diagnostics.Debug.Assert(null != (setup as SimuAO16Setup));
        //    SessionAdapter tsa = new SessionAdapter(theSession);
        //    SimuAO16OutputDeviceManager ao16 = new SimuAO16OutputDeviceManager(setup as AO308Setup, tsa);
        //    PerDeviceObjects pd = new PerDeviceObjects(realDevice);

        //    // set ao308 as consumer of udp-reader
        //    //if (_mainConfig.Blocksensor.IsActive == false)
        //    //{
        //    //    var nic = IPAddress.Parse(_mainConfig.AppSetup.SelectedNicForMCast);
        //    //    UdpReader ureader = new UdpReader(setup.LocalEndPoint.ToIpEp(), nic, ao16, ao16.InstanceName);
        //    //    pd.UdpReader = ureader;
        //    //}
        //    pd.OutputDeviceManager = ao16;

        //    return new List<PerDeviceObjects>() { pd };
        //}

        private List<PerDeviceObjects> Build_SL508(UeiDeviceInfo realDevice, DeviceSetup setup)
        {

            SL508892Setup thisSetup = setup as SL508892Setup;
            Session serialSession = null;
            try
            {
                serialSession = new Session();
                UeiCube cube = new UeiCube(thisSetup.CubeUrl);
                cube.DasReset1();

                // create serial port object for each channel
                foreach (var channel in thisSetup.Channels)
                {
                    if (false == channel.IsEnabled)
                    {
                        continue;
                    }
                    string finalUrl = $"{thisSetup.CubeUrl}Dev{thisSetup.SlotNumber}/Com{channel.ChannelIndex}";
                    SerialPort sport = serialSession.CreateSerialPort(finalUrl,
                                        channel.Mode,
                                        channel.Baudrate,
                                        SerialPortDataBits.DataBits8,
                                        channel.Parity,
                                        channel.Stopbits,
                                        "");
                    System.Diagnostics.Debug.Assert(null != sport);
                    sport.EnableErrorReporting(true);
                }

                // just verify that there are N channels (serial  ports)
                {
                    int chCount = thisSetup.Channels.Where(ch => ch.IsEnabled == true).ToList().Count;
                    System.Diagnostics.Debug.Assert(serialSession.GetNumberOfChannels() == chCount);
                }

                // set timeout for protocol
                serialSession.ConfigureTimingForMessagingIO(1000, 100.0);
                // set timeout for callback
                serialSession.GetTiming().SetTimeout(5000); // timeout to throw from _serialReader.EndRead (looks like default is 1000)

                serialSession.Start();

            }
            catch (UeiDaqException ex)
            {
                _logger.Warn($"Failed to init serial card mananger. Slot {setup.SlotNumber}. {ex.Message}. Might be invalid baud rate");
                return null;
            }

            // emit info log
            _logger.Debug($" == Serial channels for cube {setup.CubeUrl}, slot {setup.SlotNumber}");
            foreach (UeiDaq.Channel ueiChannel in serialSession.GetChannels())
            {
                SerialPort ueiPort = ueiChannel as SerialPort;
                string s1 = ueiPort.GetSpeed().ToString();
                string s2 = s1.Replace("BitsPerSecond", "");
                //SL508892Setup s508 = setup as SL508892Setup;
                int chIndex = ueiPort.GetIndex();
                int portnum = thisSetup.Channels.Where(i => i.ChannelIndex == chIndex).Select(i => i.LocalUdpPort).FirstOrDefault();
                _logger.Debug($"CH:{ueiPort.GetIndex()}, Rate {s2} bps, Mode {ueiPort.GetMode()}. Listening port {portnum}");
            }

            SessionAdapter ssAdapter = new SessionAdapter(serialSession);

            //SessionAdapter serAd = new SessionAdapter(serialSession);
            string instanceName = setup.GetInstanceName();// $"{realDevice.DeviceName}/Slot{realDevice.DeviceSlot}";
            UdpWriter uWriter = new UdpWriter(setup.DestEndPoint.ToIpEp(), _mainConfig.AppSetup.SelectedNicForMulticast);
            SL508InputDeviceManager id = new SL508InputDeviceManager(uWriter, setup, ssAdapter);

            SL508OutputDeviceManager od = new SL508OutputDeviceManager(setup, serialSession);
            var nic = IPAddress.Parse(_mainConfig.AppSetup.SelectedNicForMulticast);
            UdpReader ureader = new UdpReader(setup.LocalEndPoint.ToIpEp(), nic, _udpMessenger, od.InstanceName);
            // each port
            {
                var ip = IPAddress.Parse(setup.LocalEndPoint.Address);
                foreach (SerialChannelSetup chSetup in thisSetup.Channels)
                {
                    if (true == chSetup.IsEnabled)
                    {
                        IPEndPoint ep = new IPEndPoint(ip, chSetup.LocalUdpPort);
                        UdpReader ureader2 = new UdpReader(ep, nic, _udpMessenger, $"{od.InstanceName}/{chSetup.LocalUdpPort}");
                        _udpReaderList.Add(ureader2);
                    }
                }
            }


            var pd = new PerDeviceObjects(realDevice);
            //pd.SerialSession = serialSession;
            pd.InputDeviceManager = id;
            pd.OutputDeviceManager = od;
            //pd.UdpWriter = uWriter;

            _udpMessenger.SubscribeConsumer(od, realDevice.CubeId, realDevice.DeviceSlot);
            _udpReaderList.Add(ureader);

            return new List<PerDeviceObjects>() { pd };
        }
#if notyet
        private List<PerDeviceObjects> Build_SL508_Failsafe(UeiDeviceInfo realDevice, DeviceSetup setup)
        {
            SL508892Setup thisSetup = setup as SL508892Setup;
            SerialOp.Program p = new SerialOp.Program();
            Session serialSession = p.BuildSerialSession(thisSetup, realDevice.DeviceSlot);
            serialSession.Start();
          
            // emit info log
            _logger.Debug($" == Serial channels for cube {setup.CubeUrl}, slot {setup.SlotNumber}");
            foreach (UeiDaq.Channel ueiChannel in serialSession.GetChannels())
            {
                SerialPort ueiPort = ueiChannel as SerialPort;
                string s1 = ueiPort.GetSpeed().ToString();
                string s2 = s1.Replace("BitsPerSecond", "");
                //SL508892Setup s508 = setup as SL508892Setup;
                int chIndex = ueiPort.GetIndex();
                int portnum = thisSetup.Channels.Where(i => i.ChannelIndex == chIndex).Select(i => i.LocalUdpPort).FirstOrDefault();
                _logger.Debug($"CH:{ueiPort.GetIndex()}, Rate {s2} bps, Mode {ueiPort.GetMode()}. Listening port {portnum}");
            }

            SessionAdapter ssAdapter = new SessionAdapter(serialSession);

            //SessionAdapter serAd = new SessionAdapter(serialSession);
            string instanceName = setup.GetInstanceName();// $"{realDevice.DeviceName}/Slot{realDevice.DeviceSlot}";
            UdpWriter uWriter = new UdpWriter(setup.DestEndPoint.ToIpEp(), _mainConfig.AppSetup.SelectedNicForMulticast);
            SerialOp.SL508InputManager serialmManager = new SerialOp.SL508InputManager(uWriter, thisSetup, serialSession);
            //SL508InputDeviceManager id = new SL508InputDeviceManager(uWriter, setup, ssAdapter);

            SL508OutputDeviceManager od = new SL508OutputDeviceManager(setup, serialSession);
            var nic = IPAddress.Parse(_mainConfig.AppSetup.SelectedNicForMulticast);
            UdpReader ureader = new UdpReader(setup.LocalEndPoint.ToIpEp(), nic, _udpMessenger, od.InstanceName);
            // each port
            {
                var ip = IPAddress.Parse(setup.LocalEndPoint.Address);
                foreach (SerialChannelSetup chSetup in thisSetup.Channels)
                {
                    if (true == chSetup.IsEnabled)
                    {
                        IPEndPoint ep = new IPEndPoint(ip, chSetup.LocalUdpPort);
                        UdpReader ureader2 = new UdpReader(ep, nic, _udpMessenger, $"{od.InstanceName}/{chSetup.LocalUdpPort}");
                        _udpReaderList.Add(ureader2);
                    }
                }
            }


            var pd = new PerDeviceObjects(realDevice);
            //pd.SerialSession = serialSession;
            //pd.InputDeviceManager = id;
            pd.OutputDeviceManager = od;
            //pd.UdpWriter = uWriter;

            _udpMessenger.SubscribeConsumer(od, realDevice.CubeId, realDevice.DeviceSlot);
            _udpReaderList.Add(ureader);

            return new List<PerDeviceObjects>() { pd };
        }
#endif
        private List<PerDeviceObjects> Build_CAN503(UeiDeviceInfo realDevice, DeviceSetup setup)
        {

            CAN503Setup thisSetup = setup as CAN503Setup;

            Session canSession = null;
            try
            {
                canSession = new Session();

                {
                    foreach (CANChannelSetup channelSetup in thisSetup.Channels)
                    {
                        string finalUrl = $"{thisSetup.CubeUrl}Dev{thisSetup.SlotNumber}/CAN{channelSetup.ChannelIndex}";
                        CANPort cport = canSession.CreateCANPort(finalUrl,
                                            channelSetup.Speed,
                                            channelSetup.FrameFormat,
                                            channelSetup.PortMode,
                                            0xFFFFFFFF,
                                            0);
                        System.Diagnostics.Debug.Assert(null != cport);
                    }

                    System.Diagnostics.Debug.Assert(canSession.GetNumberOfChannels() == thisSetup.Channels.Count);

                    canSession.ConfigureTimingForMessagingIO(1, 0);
                    canSession.GetTiming().SetTimeout(100); // timeout to throw from _serialReader.EndRead (looks like default is 1000)

                    canSession.Start();
                }

            }
            catch (UeiDaqException ex)
            {
                _logger.Warn($"Failed to init serial card mananger.Slot {setup.SlotNumber}. {ex.Message}. Might be invalid baud rate");
                return null;
            }

            // emit info log
            _logger.Debug($" == Serial channels for cube {setup.CubeUrl}, slot {setup.SlotNumber}");
            foreach (UeiDaq.Channel ueiChannel in canSession.GetChannels())
            {
                //SerialPort ueiPort = ueiChannel as SerialPort;
                //string s1 = ueiPort.GetSpeed().ToString();
                //string s2 = s1.Replace("BitsPerSecond", "");
                //SL508892Setup s508 = setup as SL508892Setup;
                //int chIndex = ueiPort.GetIndex();
                //int portnum = s508.Channels.Where(i => i.ChannelIndex == chIndex).Select(i => i.LocalUdpPort).FirstOrDefault();
                CANPort cport = ueiChannel as CANPort;

                _logger.Debug($"CAN CH:{cport.GetIndex()} - {cport.GetMode()} - {cport.GetSpeed()} - {cport.GetType()}");

            }

            SessionAdapter ssAdapter = new SessionAdapter(canSession);

            //SessionAdapter serAd = new SessionAdapter(serialSession);
            string instanceName = setup.GetInstanceName();// $"{realDevice.DeviceName}/Slot{realDevice.DeviceSlot}";
            UdpWriter uWriter = new UdpWriter(setup.DestEndPoint.ToIpEp(), _mainConfig.AppSetup.SelectedNicForMulticast);
            CAN503InputDeviceManager id = new CAN503InputDeviceManager(thisSetup, ssAdapter, uWriter);

            CAN503OutputDeviceManager od = new CAN503OutputDeviceManager(setup, ssAdapter);
            var nic = IPAddress.Parse(_mainConfig.AppSetup.SelectedNicForMulticast);
            UdpReader ureader = new UdpReader(setup.LocalEndPoint.ToIpEp(), nic, _udpMessenger, od.InstanceName);
            // each port
            {
                //var add = IPAddress.Parse(setup.LocalEndPoint.Address);
                //foreach (SerialChannelSetup chSetup in thisSetup.Channels)
                //{
                //    IPEndPoint ep = new IPEndPoint(add, chSetup.LocalUdpPort);
                //    UdpReader ureader2 = new UdpReader(ep, nic, _udpMessenger, $"{od.InstanceName}/{chSetup.LocalUdpPort}");
                //    _udpReaderList.Add(ureader2);
                //}
            }

            var pd = new PerDeviceObjects(realDevice);
            pd.InputDeviceManager = id;
            pd.OutputDeviceManager = od;

            _udpMessenger.SubscribeConsumer(od, realDevice.CubeId, realDevice.DeviceSlot);
            _udpReaderList.Add(ureader);

            return new List<PerDeviceObjects>() { pd };
        }

        private List<PerDeviceObjects> Build_AI201(UeiDeviceInfo realDevice, DeviceSetup setup)
        {
            string instanceName = $"{realDevice.DeviceName}/Slot{realDevice.DeviceSlot}";
            UdpWriter uWriter = new UdpWriter(setup.DestEndPoint.ToIpEp(), _mainConfig.AppSetup.SelectedNicForMulticast);


            Session sess1 = new Session();
            string url1 = $"{setup.CubeUrl}Dev{setup.SlotNumber}/Ai0: 23";
            double peek = AI201100Setup.PeekVoltage_upstream;
            sess1.CreateAIChannel(url1, -peek, peek, AIChannelInputMode.SingleEnded); // -15,15 means 'no gain'
            //var numberOfChannels = _ueiSession.GetNumberOfChannels();
            sess1.ConfigureTimingForSimpleIO();
            sess1.Start();

            AI201InputDeviceManager id = new AI201InputDeviceManager(setup as AI201100Setup, new SessionAdapter(sess1), uWriter);

            var pd = new PerDeviceObjects(realDevice);
            //pd.UdpWriter = uWriter;
            pd.InputDeviceManager = id;

            return new List<PerDeviceObjects>() { pd };
        }

        private List<PerDeviceObjects> Build_DIO470(UeiDeviceInfo realDevice, DeviceSetup setup)
        {
            DIO470OutputDeviceManager od = new DIO470OutputDeviceManager(setup);
            var nic = IPAddress.Parse(_mainConfig.AppSetup.SelectedNicForMulticast);
            UdpReader ureader = new UdpReader(setup.LocalEndPoint.ToIpEp(), nic, _udpMessenger, od.InstanceName);

            PerDeviceObjects pd = new PerDeviceObjects(realDevice);
            pd.OutputDeviceManager = od;

            _udpMessenger.SubscribeConsumer(od, realDevice.CubeId, realDevice.DeviceSlot);
            _udpReaderList.Add(ureader);

            return new List<PerDeviceObjects>() { pd };
        }

        private List<PerDeviceObjects> Build_DIO403(UeiDeviceInfo realDevice, DeviceSetup devSetup)
        {
            DIO403Setup setup = devSetup as DIO403Setup;

            // prepare output manager
            // =======================

            // build session
            string outDevString = ComposeDio403DeviceString(realDevice, MessageWay.downstream);
            string cubeUrl = $"{setup.CubeUrl}Dev{setup.SlotNumber}/{outDevString}";
            Session outSession = new Session();
            outSession.CreateDOChannel(cubeUrl);
            outSession.ConfigureTimingForSimpleIO();
            outSession.Start();
            SessionAdapter sa1 = new SessionAdapter(outSession);

            // build device manager
            DIO403OutputDeviceManager outDev = new DIO403OutputDeviceManager(setup, sa1);

            // build udp reader
            var nic = IPAddress.Parse(_mainConfig.AppSetup.SelectedNicForMulticast);
            UdpReader ureader = new UdpReader(setup.LocalEndPoint.ToIpEp(), nic, _udpMessenger, outDev.InstanceName);
            _udpReaderList.Add(ureader);

            // Subscribe device manager as consumer to incoming messages
            _udpMessenger.SubscribeConsumer(outDev, realDevice.CubeId, realDevice.DeviceSlot);

            // prepare input manager
            // =======================

            // build udp writer
            UdpWriter udpWriter = new UdpWriter(setup.DestEndPoint.ToIpEp(), _mainConfig.AppSetup.SelectedNicForMulticast);

            // build session
            string inDevString = ComposeDio403DeviceString(realDevice, MessageWay.upstream);
            string inSessionUrl = $"{setup.CubeUrl}Dev{setup.SlotNumber}/{inDevString}";
            Session inSession = new Session();
            inSession.CreateDIChannel(inSessionUrl);
            inSession.ConfigureTimingForSimpleIO();
            inSession.Start();
            SessionAdapter sa2 = new SessionAdapter(inSession);

            // build device manager
            DIO403InputDeviceManager inDev = new DIO403InputDeviceManager(setup, sa2, udpWriter);

            // register in+out mangers
            PerDeviceObjects pd = new PerDeviceObjects(realDevice);
            pd.OutputDeviceManager = outDev;
            pd.InputDeviceManager = inDev;

            return new List<PerDeviceObjects>() { pd };
        }

        private string ComposeDio403DeviceString(UeiDeviceInfo devInfo, MessageWay way)
        {
            StringBuilder resultString = new StringBuilder((way == MessageWay.downstream) ? "Do" : "Di");
            //Do0,2,4
            DIO403Setup setup = _mainConfig.GetDeviceSetupEntry<DIO403Setup>(devInfo);
            if (null != setup)
            {
                IEnumerable<DIOChannel> l = setup.IOChannelList.Where(i => i.Way == way);
                foreach (var c in l)
                {
                    resultString.Append(c.OctetIndex);
                    resultString.Append(",");
                }
                resultString.Remove(resultString.Length - 1, 1);
            }
            else
                throw new ArgumentNullException();

            return resultString.ToString();
        }
#if dont
        public void Build_BlockSensorManager(List<UeiDeviceInfo> realDeviceList)
        {
            //DeviceSetup devSetup = null;

            foreach (CubeSetup csetup in _mainConfig.CubeSetupList)
            {
                // check if block sensor is active for cube
                BlockSensorSetup bssetup = _mainConfig.GetDeviceSetupEntry(csetup.CubeUrl, DeviceMap2.BlocksensorLiteral) as BlockSensorSetup;
                if ((null == bssetup) || (false == bssetup.IsActive))
                {
                    continue;
                }

                Session theSession = new Session();
                string cubeUrl = $"{csetup.CubeUrl}Dev{bssetup.SlotNumber}/Ao0:7";
                var ch = theSession.CreateAOChannel(cubeUrl, -AO308Setup.PeekVoltage_downstream, AO308Setup.PeekVoltage_downstream);
                System.Diagnostics.Debug.Assert(ch.GetMaximum() == AO308Setup.PeekVoltage_downstream);
                theSession.ConfigureTimingForSimpleIO();
                theSession.Start();
                //var aWriter = new AnalogWriteAdapter(new AnalogScaledWriter(theSession.GetDataStream()), theSession);
                SessionAdapter tsa = new SessionAdapter(theSession);

                BlockSensorManager2 blockSensor = new BlockSensorManager2(bssetup, tsa);

#if !blocksim
                // redirect dio430/input to block-sensor.
                IEnumerable<PerDeviceObjects> inputDevices = _PerDeviceObjectsList.Where(i => i.InputDeviceManager != null).Where(i => i.InputDeviceManager.DeviceName == DeviceMap2.DIO403Literal).Where(i => i.InputDeviceManager.DeviceInfo.DeviceSlot == bssetup.DigitalCardSlot);
                DIO403InputDeviceManager dio403 = inputDevices.Select(i => i.InputDeviceManager).FirstOrDefault() as DIO403InputDeviceManager;
                System.Diagnostics.Debug.Assert(dio403 != null);
                dio403.SetTargetConsumer(blockSensor);
#endif
                // define udp-reader for block sensor
                var nic = IPAddress.Parse(_mainConfig.AppSetup.SelectedNicForMulticast);
                UdpReader ureader = new UdpReader(bssetup.LocalEndPoint.ToIpEp(), nic, _udpMessenger, "BlockSensor");

                // add block sensor to device list
                PerDeviceObjects pd = new PerDeviceObjects(blockSensor.DeviceName, -1, "no_cube");
                pd.OutputDeviceManager = blockSensor;

                //int cubeid = UeiBridge.Library.StaticMethods.CubeUrlToIpAddress(csetup.CubeUrl).GetAddressBytes()[3]; // tbd. result of CubeUriToIpAddress might be null
                //_udpMessenger.SubscribeConsumer(blockSensor, cubeid, 32);
                //_udpReaderList.Add(ureader);

                blockSensor.OpenDevice();
                //ureader.Start();

#if blocksim
                // send single message to block sensor
                //byte[] d403 = Library.StaticMethods.Make_Dio403_upstream_message(new byte[] { 0x5, 0, 0 });
                //string err=null;
                var ethMsg = EthernetMessage.CreateMessage(DeviceMap2.GetDeviceName(DeviceMap2.DIO403Literal), 1,2, new byte[] { 0x5, 0, 0, 0, 0, 0 });
                blockSensor.Enqueue(ethMsg.GetByteArray(MessageWay.upstream));
#endif
                _PerDeviceObjectsList.Add(pd);

            }


        }
#endif
        public void Dispose()
        {
            List<Task> tl = new List<Task>();
            foreach (var entry in _PerDeviceObjectsList)
            {
                tl.Add(
                    Task.Factory.StartNew(() =>
                    {
                        _logger.Debug($"Start Disposing {entry.DeviceName}");
                        entry.OutputDeviceManager?.Dispose();
                        entry.InputDeviceManager?.Dispose();
                        _logger.Debug($"Finished Disposing {entry.DeviceName}");
                        //entry.UdpWriter?.Dispose();
                    })
                );
                System.Threading.Thread.Sleep(10);

            }
            Task.WaitAll(tl.ToArray());
            foreach (var entry in _udpReaderList)
            {
                entry.Dispose();
            }

            foreach(var entry in _deviceManagerList)
            {
                //entry.Dispose();
            }
        }
    }
}
