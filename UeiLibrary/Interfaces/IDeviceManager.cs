using System;

namespace UeiBridge.Library.Interfaces
{
    public interface IDeviceManager: IDisposable
    {
        string DeviceName { get; }
        string InstanceName { get; }
        string[] GetFormattedStatus(TimeSpan interval);
    }


}
