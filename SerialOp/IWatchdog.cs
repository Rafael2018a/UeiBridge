using System;

namespace SerialOp
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
