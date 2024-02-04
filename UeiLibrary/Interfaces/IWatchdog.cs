using System;

namespace UeiBridge.Library.Interfaces
{
    public interface IWatchdog: IDisposable
    {
        void Register(string v, TimeSpan timeSpan);
        void NotifyAlive(string v);
        void NotifyCrash(string source, string reason);
        //void StartWatching();
        //void StopWatching();
    }
}
