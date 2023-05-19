using System;
using System.Threading;
using UeiDaq;
using UeiBridge.Types;
using UeiBridge.Library;

namespace UeiBridge
{
    public abstract class InputDevice : IDeviceManager, IDisposable
    {
        public abstract string [] GetFormattedStatus( TimeSpan interval);
        public abstract void OpenDevice();
        public abstract string DeviceName { get; } 

        public string InstanceName { get; private set; }
        public UeiDeviceInfo DeviceInfo { get; private set; }

        //private log4net.ILog _logger = StaticMethods.GetLogger();

        public InputDevice() { }
        protected InputDevice( DeviceSetup setup)
        {
            System.Diagnostics.Debug.Assert(null != setup);
            InstanceName = $"{DeviceName}/Cube{setup.CubeId}/Slot{setup.SlotNumber}/Input";
            //SlotNumber = setup.SlotNumber;

            DeviceInfo = new UeiDeviceInfo(setup.CubeUrl, DeviceName, setup.SlotNumber);
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

        public abstract void Dispose();
        
    }
}
