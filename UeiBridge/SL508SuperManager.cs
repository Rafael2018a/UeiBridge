using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UeiBridge.Library;
using UeiBridge.Library.CubeSetupTypes;
using UeiDaq;

namespace SerialOp
{
    /// <summary>
    /// This class is kind or Decorator to SL508DeviceManger,
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

        public void Dispose()
        {
            _stopByDispose = true;
            _deviceManager.Dispose();
        }
        public string[] GetFormattedStatus(TimeSpan interval)
        {
            return _deviceManager?.GetFormattedStatus(interval);
        }
        public SL508SuperManager()
        {

        }
        public void StartDevice(SL508892Setup deviceSetup)
        {
            if (null != deviceSetup)
            {
                Task.Run(() => WatchdogLoop(deviceSetup));
                //Task.Factory.StartNew(() => WatchdogLoop(deviceSetup));
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
                    break;// loop
                }
                serSession.Start();

                _logger.Info($"Listening on {serSession.GetDevice().GetResourceName()}");

                // set device manager and watchdog
                // -------------------------------
                _deviceManager = new SL508DeviceManager(null, deviceSetup, serSession);
                DeviceWatchdog wd = new DeviceWatchdog(new Action<string, string>((source, reason) => 
                { 
                    _stopByWatchdog = true;
                    _logger.Warn($"WD reset by {source}. Reason: {reason}");
                }));

                _deviceManager.SetWatchdog(wd);
                if (false == _deviceManager.StartDevice())
                {
                    _logger.Info("Failed to start device");
                    break;// loop
                }
                
                // 
                do
                {
                    Task.Delay(100).Wait();
                } while (_stopByWatchdog == false && _stopByDispose == false);

                // Display statistics 
                _logger.Info("Serial statistics\n--------");
                foreach (var ch in _deviceManager.ChannelStatList)
                {
                    //_logger.Info($"CH {ch.ChannelIndex}: {ch.ToString()}");
                }
                // dispose process
                // ----------------
                _deviceManager.Dispose();

                serSession.Stop();
                System.Diagnostics.Debug.Assert(false == serSession.IsRunning());
                serSession.Dispose();
                serSession = null;

                _logger.Info(" = Dispose fin =");

                // wait before restart after WD reset
                if (true == _stopByWatchdog)
                {
                    System.Threading.Thread.Sleep(1000);
                    _stopByWatchdog = false;
                }
                
            } while (false == _stopByDispose);

        }
    }
}
