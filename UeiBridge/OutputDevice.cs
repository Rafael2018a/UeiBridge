using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using UeiDaq;

/// <summary>
/// All files in project might refer to this file.
/// Types in this file might NOT refer to types in any other file.
/// </summary>
namespace UeiBridge
{
    public abstract class OutputDevice : IEnqueue<DeviceRequest>
    {
        BlockingCollection<DeviceRequest> _dataItemsQueue = new BlockingCollection<DeviceRequest>(100); // max 100 items
        //protected string _deviceIndex;
        protected string _channelsString;
        protected Session _deviceSession;
        protected string _caseUrl;
        protected string _deviceName;// = "AO-308";
        public string DeviceName => _deviceName;
        protected int _numberOfChannels = 0;
        //public int NumberOfChannels => _numberOfChannels;
        public virtual void CloseDevice()
        {
            if (null != _deviceSession)
            {
                _deviceSession.Stop();
                _deviceSession.Dispose();
            }
            _deviceSession = null;
        }
        protected abstract void HandleRequest(DeviceRequest request);

        public void Enqueue(DeviceRequest dr)
        {
            _dataItemsQueue.Add(dr);
        }
        public void Start()
        {
            Task.Factory.StartNew(() => OutputDeviceHandler_Task());
        }

        protected void OutputDeviceHandler_Task()
        {
            while (false == _dataItemsQueue.IsCompleted)
            {
                // get from q
                DeviceRequest incomingRequest = _dataItemsQueue.Take();
                HandleRequest(incomingRequest);
            }
        }
    }
}
