using System;
using System.Collections.Generic;
using System.ComponentModel;
using UeiBridge.Library.Interfaces;

namespace UeiBridge.Library
{
    /// <summary>
    /// Serial channel watchdog.
    /// Each entry gets its own one-shot-timer, when one of the timers elapses, watchdog event is called.
    /// The watchdog action might be triggered by two events:
    /// 1. No keep alive in one of the channel
    /// 2. NotifyCrash called from one of the channels.
    /// After one of the timers elapsed and an action was called, all other timers are disabled.
    /// </summary>
    public class DeviceWatchdog : IWatchdog
    {
        Dictionary<string, System.Timers.Timer> _wdDic = new Dictionary<string, System.Timers.Timer>();
        Action<string, string> _wdAction;
        bool _disposeRequested = false;
        bool _singleTimerElapsed = false;
        bool _crashNotified = false;
        object _lockObject = new object();

        bool IsWatchingStopped() { return (_disposeRequested==true || _singleTimerElapsed==true || _crashNotified==true); }
        public DeviceWatchdog(Action<string, string> action)
        {
            System.Diagnostics.Debug.Assert(_disposeRequested == false);
            _wdAction = action;
            System.Diagnostics.Debug.Assert(null != action);
        }
        /// <summary>
        /// This will cause immediate action
        /// </summary>
        public void NotifyCrash(string originator, string reason)
        {
            if (true == IsWatchingStopped())
            {
                return;
            }
            lock (_lockObject)
            {
                _crashNotified = true;
                _wdAction(originator, reason);
            }
        }
        /// <summary>
        /// Restart client timer.
        /// should be called periodically by client.
        /// </summary>
        public void NotifyAlive(string originator)
        {
            System.Timers.Timer t;
            lock (_lockObject)
            {
                if (_wdDic.TryGetValue(originator, out t))
                {
                    // restart
                    t.Stop();
                    t.Start();
                }
                else
                {
                    if (false == IsWatchingStopped())
                    {
                        Console.WriteLine($"{originator} is not registered");
                    }
                }
            }
        }
        /// <summary>
        /// Register client for watch dog services
        /// </summary>
        public void Register(string originator, TimeSpan timeSpan)
        {
            if (true == IsWatchingStopped())
            {
                Console.WriteLine("Can't register watching stopped");
                return;
            }
            lock (_lockObject)
            {
                System.Timers.Timer t = new System.Timers.Timer();
                t.AutoReset = false;
                t.Interval = timeSpan.TotalMilliseconds;
                t.Elapsed += Timer_Elapsed;
                t.Site = new Site1(originator);
                t.Start();

                _wdDic.Add(originator, t);
            }
        }

        
        /// <summary>
        /// Watchdog timer for specific client elapsed.
        /// </summary>
        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            lock (_lockObject)
            {
                if (true == IsWatchingStopped())
                {
                    return;
                }
                _singleTimerElapsed = true;
                System.Timers.Timer t = sender as System.Timers.Timer;
                //Console.WriteLine($"{t.Site.Name} is not alive..");
                _wdAction(t.Site.Name, "Not alive");
            }
        }

        public void Dispose()
        {
            _disposeRequested = true;
            //foreach (var tm in _wdDic)
            //{
            //    tm.Value.Stop();
            //}
            //_wdDic.Clear();
            //_wdDic = null;
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
