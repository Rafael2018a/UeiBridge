using System;
using System.Threading;
using UeiDaq;
using UeiBridge.Types;
using UeiBridge.Library;

/// <summary>
/// All files in project might refer to this file.
/// Types in this file might NOT refer to types in any other file.
/// </summary>
namespace UeiBridge
{
    public abstract class InputDevice : IDeviceManager, IDisposable
    {
        public abstract string [] GetFormattedStatus( TimeSpan interval);
        public abstract void OpenDevice();
        public abstract string DeviceName { get; } 

        public string InstanceName { get; private set; }
        public int SlotNumber { get; private set; }
        private log4net.ILog _logger = StaticMethods.GetLogger();

        public InputDevice() { }
        protected InputDevice( DeviceSetup setup)
        {
            if (null != setup)
            {
                InstanceName = $"{DeviceName}/Cube{setup.CubeId}/Slot{setup.SlotNumber}/Input";
            }
            else
            {
                InstanceName = "<undefined input device>";
            }
            SlotNumber = setup.SlotNumber;
        }
        public static void CloseSession(Session theSession)
        {
            if (null != theSession)
            {
                if (theSession.IsRunning())
                {
                    theSession.Stop();
                }
                theSession.Dispose();
            }
        }

        public virtual void Dispose()
        {
            _logger.Debug($"Device manager {InstanceName} Disposed");
        }
    }
}
