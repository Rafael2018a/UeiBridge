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
    class SL508SuperManager : UeiBridge.Library.Interfaces.IDeviceManager
    {
        public string DeviceName => throw new NotImplementedException();

        public string InstanceName => throw new NotImplementedException();

        public void Dispose()
        {
            stopByDispose = true;
            deviceManager.Dispose();
        }

        public string[] GetFormattedStatus(TimeSpan interval)
        {
            throw new NotImplementedException();
        }

        SL508DeviceManager deviceManager;
        bool stopByWatchdog = false;
        bool stopByDispose = false;

        public SL508SuperManager()
        {

        }

        public void StartDevice(SL508892Setup deviceSetup)
        {
            if (null != deviceSetup)
            {
                Task.Factory.StartNew(() => WatchdogLoop(deviceSetup));
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

                Console.WriteLine($"Listening on {serSession.GetDevice().GetResourceName()}");

                // set device manager and watchdog
                // -------------------------------
                deviceManager = new SL508DeviceManager(null, deviceSetup, serSession);
                //SerialWatchdog swd = new SerialWatchdog(new Action<string>((i) => { stopByWatchdog = true; deviceManager.Dispose(); }));
                //deviceManager.SetWatchdog(swd);
                if (false == deviceManager.StartDevice())
                {
                    break;// loop
                }
                deviceManager.WaitAll();

                // Display statistics 
                Console.WriteLine("Serial statistics\n--------");
                foreach (var ch in deviceManager.ChannelStatList)
                {
                    Console.WriteLine($"CH {ch.ChannelIndex}: {ch.ToString()}");
                }
                // dispose process
                // ----------------
                //deviceManager.Dispose();
                deviceManager = null;

                serSession.Stop();
                System.Diagnostics.Debug.Assert(false == serSession.IsRunning());
                serSession.Dispose();
                serSession = null;

                Console.WriteLine(" = Dispose fin =");

                // wait before restart
                if (true == stopByWatchdog)
                {
                    System.Threading.Thread.Sleep(1000);
                    stopByWatchdog = false;
                }
                //swd = null;
            } while (false == stopByDispose);

        }
    }
}
