using System;
using System.Threading;
using UeiDaq;

/// <summary>
/// All files in project might refer to this file.
/// Types in this file might NOT refer to types in any other file.
/// </summary>
namespace UeiBridge
{
    public abstract class InputDevice : IDeviceManager, IDisposable
    {

        protected Session _deviceSession;
        protected string _cubeUrl;
        //protected string _deviceName;// = "AO-308";
        protected string _channelsString;
        //protected IConvert _attachedConverter;
        public abstract IConvert AttachedConverter { get; }
        protected readonly ISend<SendObject> _targetConsumer;
        DeviceSetup _deviceSetup;
        public abstract void Start();
        public abstract string GetFormattedStatus();
        //protected InputDevice(IEnqueue<ScanResult> targetConsumer, TimeSpan samplingInterval, string cubeUrl)
        //{
        //    _targetConsumer = targetConsumer;
        //    _samplingInterval = samplingInterval;
        //    _cubeUrl = cubeUrl;
        //}
        protected InputDevice( ISend<SendObject> targetConsumer, DeviceSetup setup)
        {
            _targetConsumer = targetConsumer;
            _deviceSetup = setup;
        }
        public abstract string DeviceName { get; }
        protected System.Threading.Timer _samplingTimer;
        protected TimeSpan _samplingInterval;
        public virtual void CloseDevice()
        {
            if (null != _deviceSession)
            {
                _deviceSession.Stop();
                _deviceSession.Dispose();
            }
            _deviceSession = null;
        }

        public abstract void Dispose();

        //public abstract int getme {get;}
    }
}
