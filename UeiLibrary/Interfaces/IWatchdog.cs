using System;

namespace UeiBridge.Library.Interfaces
{
    public interface IWatchdog
    {
        void Register(string v, TimeSpan timeSpan);
        void NotifyAlive(string v);
        void NotifyCrash(string v);
        //void StartWatching();
        void StopWatching();
    }
}
