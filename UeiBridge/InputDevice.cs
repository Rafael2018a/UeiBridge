using System;
using System.Threading;
using UeiDaq;
using UeiBridge.Types;
using UeiBridge.Library;
using UeiBridge.CubeSetupTypes;
using UeiBridge.Interfaces;

/// <summary>
/// All files in project might refer to this file.
/// Types in this file might NOT refer to types in any other file.
/// </summary>
namespace UeiBridge
{
    public abstract class InputDevice : IDeviceManager, IDisposable
    {
        public abstract string [] GetFormattedStatus( TimeSpan interval);
        public abstract bool OpenDevice();
        public abstract string DeviceName { get; }

        public UeiDeviceInfo DeviceInfo { get; private set; }
        public string InstanceName { get; private set; }
        //public int SlotNumber { get; private set; }
        private log4net.ILog _logger = StaticMethods.GetLogger();
        protected ISession _iSession;// { get; set; }
        protected ISend<SendObject> _targetConsumer;// { get ; set; }
        //protected bool _isDeviceReady = false;

        public InputDevice() { }
        protected InputDevice( DeviceSetup setup)
        {
            InstanceName = setup.GetInstanceName() + "/Input";
            DeviceInfo = setup.GetDeviceInfo();
            //SlotNumber = setup.SlotNumber;
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
        //{
        //    //_ueiSession?.Dispose();
        //    TargetConsumer?.Dispose();
        //    _logger.Debug($"Device manager {InstanceName} Disposed");
        //}

        protected void EmitInitMessage(string deviceMessage)
        {
            _logger.Info($"Cube{DeviceInfo.CubeId}/Slot{DeviceInfo.DeviceSlot}: {deviceMessage}");
        }

    }
}
