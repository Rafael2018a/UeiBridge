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
        // abstarcts
        public virtual IConvert AttachedConverter { get; }
        public abstract string [] GetFormattedStatus( TimeSpan interval);
        public abstract void OpenDevice();
        public abstract string DeviceName { get; }
        public string InstanceName { get; }

        // protected
        protected Session _deviceSession;
        protected string _cubeUrl; // tbd. remove this
        protected string _channelsString;
        protected ISend<SendObject> _targetConsumer;
        protected System.Threading.Timer _samplingTimer;
        //protected DateTime _publishTime = DateTime.Now;

        //protected DeviceSetup _deviceSetup;

        //protected InputDevice(IEnqueue<ScanResult> targetConsumer, TimeSpan samplingInterval, string cubeUrl)
        //{
        //    _targetConsumer = targetConsumer;
        //    _samplingInterval = samplingInterval;
        //    _cubeUrl = cubeUrl;
        //}
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
            //_deviceSetup = setup;
        }
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
