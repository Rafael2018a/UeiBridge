using System;
using UeiDaq;

/// <summary>
/// All files in project might refer to this file.
/// Types in this file might NOT refer to types in any other file.
/// </summary>
namespace UeiBridge
{
    public abstract class InputDevice 
    {
        
        protected Session _deviceSession;
        protected string _caseUrl;
        protected string _deviceName;// = "AO-308";
        protected int _numberOfChannels = 0;
        protected string _channelsString;
        protected IConvert _attachedConverter;
        public IConvert AttachedConverter => _attachedConverter;

        public string DeviceName => _deviceName; 

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
    }
}
