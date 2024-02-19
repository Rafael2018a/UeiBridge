﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UeiBridge.Library;
using UeiBridge.Library.CubeSetupTypes;
using UeiDaq;

namespace UeiBridge
{
    /// <summary>
    /// This class is kind of Decorator to SL508DeviceManger,
    /// it adds a watchdog ability. Meaning, that if SL508DeviceManger crashes or 
    /// stop functioning from any reason, this class shall restart the SL508DeviceManger
    /// </summary>
    public class SL508SuperManager : UeiBridge.Library.Interfaces.IDeviceManager
    {
        SL508DeviceManager _deviceManager; // decorated object
        bool _stopByWatchdog = false;
        bool _stopByDispose = false;
        public string DeviceName => _deviceManager?.DeviceName;
        public string InstanceName => _deviceManager?.InstanceName;
        readonly log4net.ILog _logger = log4net.LogManager.GetLogger("SL508Super");
        string _selectedNIC;
        public void Dispose()
        {
            _stopByDispose = true;
            _deviceManager.Dispose();
        }
        public string[] GetFormattedStatus(TimeSpan interval)
        {
            return _deviceManager?.GetFormattedStatus(interval);
        }
        public SL508SuperManager() { }// must have empty c-tor
        public SL508SuperManager(string selectedNIC)
        {
            _selectedNIC = selectedNIC;
        }
        public void StartDevice(SL508892Setup deviceSetup)
        {
            if (null != deviceSetup)
            {
                Task t = Task.Run(() => Task_WatchdogLoop(deviceSetup));
                do
                {
                    System.Threading.Thread.Sleep(50);
                } while (t.Status != TaskStatus.Running);
                System.Threading.Thread.Sleep(5000);
            }
        }
        void Task_WatchdogLoop(SL508892Setup deviceSetup)
        {
            System.Threading.Thread.CurrentThread.Name = "Task# Serial WatchdogLoop";
            _logger.Debug($"{System.Threading.Thread.CurrentThread.Name} start");
            do // watchdog loop
            {
                List<UdpReader> udpReaderList = new List<UdpReader>();

                // set session
                // -----------
                Session serSession = SL508DeviceManager.BuildSerialSession2(deviceSetup);
                if (null == serSession)
                {
                    break; // WD loop
                }
                serSession.Start();
                UeiDevice udevice = new UeiDevice(serSession.GetDevice().GetResourceName());
                _logger.Info($" == Opening Cube{udevice.GetCubeId()}{udevice.LocalPath} SL508 (Serial) == ");

                string writerName = $"SL508/Cube{udevice.GetCubeId()}/{udevice.LocalPath}";
                // defince writer for upstream process
                UdpWriterAsync uWriter = new UdpWriterAsync(deviceSetup.DestEndPoint.ToIpEp(), _selectedNIC, writerName);

                // create SL device manager 
                // -------------------------------
                _deviceManager = new SL508DeviceManager(uWriter, deviceSetup, serSession);

                // define watchdog handler
                DeviceWatchdog wd = new DeviceWatchdog(new Action<string, string>((source, reason) =>
                {
                    _stopByWatchdog = true;
                    _logger.Warn($"WD reset by {source}. Reason: {reason}");
                }));
                if (deviceSetup.EnableWatchdog)
                {
                    _deviceManager.SetWatchdog(wd);
                }

                // Create udp readers for downstream processing
                // ----------------------------------------------
                {
                    UdpToSlotMessenger u2s = new UdpToSlotMessenger();
                    u2s.SubscribeConsumer(_deviceManager, deviceSetup.GetCubeId(), deviceSetup.SlotNumber);

                    var ip4all = IPAddress.Parse(deviceSetup.LocalEndPoint.Address);
                    foreach (SerialChannelSetup channelSetup in deviceSetup.Channels)
                    {
                        if (true == channelSetup.IsEnabled) // if com enabled
                        {
                            IPEndPoint ep = new IPEndPoint(ip4all, channelSetup.LocalUdpPort);
                            UdpReader ureader2 = new UdpReader(ep, null, u2s, $"InstanceName");
                            ureader2.Start();
                            _logger.Info($"Cube{deviceSetup.GetCubeId()}/Dev{deviceSetup.SlotNumber}/Com{channelSetup.ComIndex} Writer ready. Listening on {ep.ToString()}");
                            udpReaderList.Add(ureader2);
                        }
                    }
                }

                if (false == _deviceManager.StartDevice())
                {
                    _logger.Info("Failed to start device");
                    break;// WD loop
                }

                // wait, as long as device-manager runs
                do
                {
                    System.Threading.Thread.Sleep(100);
                } while (_stopByWatchdog == false && _stopByDispose == false);

                // dispose process
                // ----------------
                uWriter.Dispose();
                _deviceManager.Dispose();
                foreach(var ent in udpReaderList)
                {
                    ent.Dispose();
                }

                serSession.Stop();
                System.Diagnostics.Debug.Assert(false == serSession.IsRunning());
                serSession.Dispose();
                serSession = null;

                _logger.Info("SL508Super disposed");

                // in case of WD reset, wait before restart.
                if (true == _stopByWatchdog)
                {
                    System.Threading.Thread.Sleep(1000);
                    _stopByWatchdog = false;
                }

            } while (false == _stopByDispose);

            _logger.Debug($"{System.Threading.Thread.CurrentThread.Name} end");
        }

    }
}
