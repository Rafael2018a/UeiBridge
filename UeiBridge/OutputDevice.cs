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
    public abstract class OutputDevice :  IDisposable, IEnqueue<byte[]> // IEnqueue<DeviceRequest>,
    {
        // abstracts properties
        // -------------------
        protected abstract string ChannelsString { get; }
        public abstract string DeviceName { get; }
        public abstract string InstanceName { get; }
        protected abstract IConvert AttachedConverter { get; }

        // abstract methods
        // ----------------
        public abstract bool OpenDevice();
        protected abstract void HandleRequest(EthernetMessage request);
        public abstract string GetFormattedStatus();
        public abstract void Dispose();

        // fields
        // --------
        private BlockingCollection<EthernetMessage> _dataItemsQueue2 = new BlockingCollection<EthernetMessage>(100); // max 100 items
        log4net.ILog _logger = StaticMethods.GetLogger();
        protected Session _deviceSession;
        protected string _caseUrl; // remove?
        public static string CancelTaskRequest => "canceltoken"; // tbd. use standard CanceltationToken
        protected DeviceSetup _deviceSetup; // from config
        protected bool _isDeviceReady=false;

        protected OutputDevice(DeviceSetup deviceSetup)
        {
            _deviceSetup = deviceSetup;
        }
        public virtual void CloseSession()
        {
            if (null != _deviceSession)
            {
                _deviceSession.Stop();
                _deviceSession.Dispose();
            }
            _deviceSession = null;
        }
        public void Enqueue(byte[] m)
        {
            string errorString;
            EthernetMessage em = EthernetMessage.CreateFromByteArray(m, out errorString);
            if (em != null)
            {
                _dataItemsQueue2.Add(em);
            }
            else
            {
                _logger.Warn($"Incoming byte message error. {errorString}. message dropped.");
            }
        }

        //public virtual void Start1()
        //{
        //    Task.Factory.StartNew(() => OutputDeviceHandler_Task());
        //}

        protected void OutputDeviceHandler_Task()
        {
            _logger.Debug($"OutputDeviceHandler_Task {DeviceName}");
            // message loop
            while (false == _dataItemsQueue2.IsCompleted)
            {
                // get from q
                EthernetMessage incomingMessage = _dataItemsQueue2.Take();
                System.Diagnostics.Debug.Assert(null != incomingMessage);
                //if (incomingMessage.RequestObject.ToString() == OutputDevice.CancelTaskRequest) // tbd. fix this
                //{
                //    break;
                //}
                if (_isDeviceReady)
                {
                    HandleRequest(incomingMessage);
                }
                else
                {
                    _logger.Warn($"Device {DeviceName} not ready. message dropped.");
                }
            }

            _logger.Debug($"OutputDeviceHandler_Task end {this.GetType().ToString()}");
        }
                
    }
}
