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
        public List<PerDeviceObjects> _deviceManagers = new List<PerDeviceObjects>();
        public void CreateDeviceManagers( List<DeviceEx> readDeviceList)
        {
            foreach( DeviceEx realDevice in readDeviceList)
            {
                // prologue
                // =========
                var t = StaticMethods.GetDeviceManagerType<IDeviceManager>(realDevice.PhDevice.GetDeviceName()); // it type exists
                if (null==t)
                {
                    _logger.Info($"Device of type {realDevice.PhDevice.GetDeviceName()} not supported");
                    continue;
                }
                DeviceSetup setup = Config2.Instance.GetSetupEntryForDevice(realDevice.CubeUrl, realDevice.PhDevice.GetIndex()); // if config entry exists
                if (null == setup)
                {
                    _logger.Warn($"No config entry for Device {realDevice.PhDevice.GetDeviceName()}");
                    continue;
                }
                if (setup.DeviceName != realDevice.PhDevice.GetDeviceName()) // if config entry match
                {
                    _logger.Warn($"Config entry at slot {realDevice.PhDevice.GetIndex()}/ Cube {realDevice.CubeUrl} does not match physical device {realDevice.PhDevice.GetDeviceName()}");
                    continue;
                }

                List<PerDeviceObjects> objs = BuildObjectsForDevice(realDevice, setup);
                _deviceManagers.AddRange(objs);

            }
        }

        private List<PerDeviceObjects> BuildObjectsForDevice(DeviceEx realDevice, DeviceSetup setup)
        {
            List<PerDeviceObjects> result = new List<PerDeviceObjects>();
            switch ( realDevice.PhDevice.GetDeviceName())
            {
                case "Simu-AO16":
                    {
                        DeviceSetup deviceSetup = setup;
                        Type devType = StaticMethods.GetDeviceManagerType<IDeviceManager>(realDevice.PhDevice.GetDeviceName());
                        OutputDevice outDev = (OutputDevice)Activator.CreateInstance(devType, deviceSetup);

                        System.Diagnostics.Debug.Assert(null != outDev);
                        System.Diagnostics.Debug.Assert(null != deviceSetup.LocalEndPoint);

                        // create udp reader for this device
                        var nic = IPAddress.Parse(Config2.Instance.AppSetup.SelectedNicForMCast);
                        UdpReader ureader = new UdpReader(deviceSetup.LocalEndPoint.ToIpEp(), nic, outDev, outDev.InstanceName);

                        // update device table
                        result.Add(new PerDeviceObjects(realDevice, outDev, ureader));
                    }
                    break;
            }

            return result;
            
        }


    }
}
