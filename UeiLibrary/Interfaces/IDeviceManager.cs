using System;

namespace UeiBridge.Interfaces
{
    public interface IDeviceManager
    {
        string DeviceName { get; }
        string InstanceName { get; }
        string[] GetFormattedStatus(TimeSpan interval);
    }


}
