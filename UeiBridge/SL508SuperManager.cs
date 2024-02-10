using System;
using System.Collections.Generic;
using System.Linq;
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
        readonly log4net.ILog _logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
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
		public SL508SuperManager(){}// must have empty c-tor
        public SL508SuperManager( string selectedNIC)
        {
            _selectedNIC = selectedNIC;
        }
        public void StartDevice(SL508892Setup deviceSetup)
        {
            if (null != deviceSetup)
            {
                Task.Run(() => WatchdogLoop(deviceSetup));
            }
        }
        void WatchdogLoop(SL508892Setup deviceSetup)
        {
            do // watchdog loop
            {
                // set session
                // -----------
                Session serSession = SL508DeviceManager.BuildSerialSession2(deviceSetup);
                if (null == serSession)
                {
                    break; // WD loop
                }
                serSession.Start();

                

                UdpWriterAsync uWriter = new UdpWriterAsync(deviceSetup.DestEndPoint.ToIpEp(), _selectedNIC);//, _mainConfig.AppSetup.SelectedNicForMulticast);

                // set device manager and watchdog
                // -------------------------------
                _deviceManager = new SL508DeviceManager(uWriter, deviceSetup, serSession);
                DeviceWatchdog wd = new DeviceWatchdog(new Action<string, string>((source, reason) => 
                { 
                    _stopByWatchdog = true;
                    _logger.Warn($"WD reset by {source}. Reason: {reason}");
                }));

                _deviceManager.SetWatchdog(wd);
                if (false == _deviceManager.StartDevice())
                {
                    _logger.Info("Failed to start device");
                    break;// WD loop
                }
                
                // wait, as long as device-manager runs
                do
                {
                    Task.Delay(100).Wait();
                } while (_stopByWatchdog == false && _stopByDispose == false);

                // Display statistics b4 termination
                _logger.Info("Serial statistics\n--------");
                foreach (var ch in _deviceManager.ChannelStatList)
                {
                    //_logger.Info($"CH {ch.ChannelIndex}: {ch.ToString()}");
                }

                // dispose process
                // ----------------
                uWriter.Dispose();
                _deviceManager.Dispose();

                serSession.Stop();
                System.Diagnostics.Debug.Assert(false == serSession.IsRunning());
                serSession.Dispose();
                serSession = null;

                _logger.Info(" = Dispose fin =");

                // in case of WD reset, wait before restart.
                if (true == _stopByWatchdog)
                {
                    System.Threading.Thread.Sleep(1000);
                    _stopByWatchdog = false;
                }
                
            } while (false == _stopByDispose);

        }
    }
}
