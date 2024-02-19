using System;
using System.Collections.Generic;
using System.ComponentModel;
using UeiBridge.Library.Interfaces;

namespace UeiBridge.Library
{
    /// <summary>
    /// Serial channel watchdog.
    /// The watchdog action might be triggered by two events:
    /// 1. No keep alive in one of the channel
    /// 2. NotifyCrash called from one of the channels.
    /// Call StopWatching() if you want to disable watching.
    /// </summary>
    public class SerialWatchdog : IWatchdog
    {
        Dictionary<string, System.Timers.Timer> _wdDic = new Dictionary<string, System.Timers.Timer>();
        Action<string> _wdAction;
        bool _stopWatching = false;
        bool _actionActivated = false; // action might be called only once.

        public SerialWatchdog(Action<string> action)
        {
            _wdAction = action;
            System.Diagnostics.Debug.Assert(null != action);
        }
        /// <summary>
        /// This will cause immediate action
        /// </summary>
        public void NotifyCrash(string originator)
        {
            if (true == _stopWatching)
            {
                return;
            }
            Console.WriteLine($"{originator} Crash");
            DoWatchdogAction(originator);
        }

        /// <summary>
        /// This should be called periodically by client.
        /// </summary>
        public void NotifyAlive(string originator)
        {
            System.Timers.Timer t;
            if (_wdDic.TryGetValue(originator, out t))
            {
                t.Stop();
                if (false == _stopWatching)
                {
                    t.Start();
                }
            }
            else
            {
                Console.WriteLine($"{originator} is not registered");
            }
        }

        /// <summary>
        /// Register client for watch dog services
        /// </summary>
        public void Register(string originator, TimeSpan timeSpan)
        {
            if (true == _stopWatching)
            {
                return;
            }
            System.Timers.Timer t = new System.Timers.Timer();
            t.AutoReset = false;
            t.Interval = timeSpan.TotalMilliseconds;
            t.Elapsed += Timer_Elapsed;
            t.Site = new Site1(originator);
            t.Start();

            _wdDic.Add(originator, t);
        }

        /// <summary>
        /// Watchdog timer for specific client elapsed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (true == _stopWatching)
            {
                return;
            }
            System.Timers.Timer t = sender as System.Timers.Timer;
            Console.WriteLine($"{t.Site.Name} is not alive..");
            DoWatchdogAction(t.Site.Name);
        }

        public void StopWatching()
        {
            _stopWatching = true;
        }

        void DoWatchdogAction(string eventName)
        {
            System.Diagnostics.Debug.Assert(null != _wdAction);
            if (false == _actionActivated)
            {
                _actionActivated = true;
                Console.WriteLine($"{eventName} WD action in 5 seconds...");
                System.Threading.Thread.Sleep(5000);
                _wdAction(eventName);
            }
        }
    }
    /// <summary>
    /// This class is defined just for the Name field.
    /// The aim is to know which timer (name) elapsed
    /// </summary>
    class Site1 : ISite
    {
        public Site1(string n)
        {
            this.Name = n;
        }
        public IComponent Component => throw new NotImplementedException();

        public IContainer Container => throw new NotImplementedException();

        public bool DesignMode { get; set; }

        public string Name { get; set; }

        public object GetService(Type serviceType)
        {
            throw new NotImplementedException();
        }
    }

}
