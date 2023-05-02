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
        public virtual IConvert AttachedConverter { get; }
        public abstract string [] GetFormattedStatus( TimeSpan interval);
        public abstract void OpenDevice();
        public abstract string DeviceName { get; }
        public string InstanceName { get; }
        public DeviceSetup ThisDeviceSetup { get; private set; }

        protected Session _deviceSession;
        protected string _channelsString;
        protected ISend<SendObject> _targetConsumer;
        protected System.Threading.Timer _samplingTimer;

        private log4net.ILog _logger = StaticMethods.GetLogger();

        protected InputDevice(ISend<SendObject> targetConsumer, DeviceSetup setup)
        {
            _targetConsumer = targetConsumer;
            if (null != setup)
            {
                InstanceName = $"{DeviceName}/Cube{setup.CubeId}/Slot{setup.SlotNumber}/Input";
            }
            else
            {
                InstanceName = "<undefined input device>";
            }
            ThisDeviceSetup = setup;
        }
        public virtual void CloseCurrentSession()
        {
            if (null != _deviceSession)
            {
                if (_deviceSession.IsRunning())
                {
                    _deviceSession.Stop();
                }
                _deviceSession.Dispose();
            }
        }

        public virtual void Dispose()
        {
            _logger.Debug($"Disposing {this.DeviceName}/Input, slot {ThisDeviceSetup.SlotNumber}");
        }
    }
}
