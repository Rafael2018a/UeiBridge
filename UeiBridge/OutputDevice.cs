using System;
using System.Collections.Concurrent;
using System.Threading;
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
        public abstract string DeviceName { get; }
        public abstract string InstanceName { get; }

        // abstract methods
        // ----------------
        public abstract bool OpenDevice();
        protected abstract void HandleRequest(EthernetMessage request);
        public abstract string GetFormattedStatus();
        
        // fields
        // --------
        private BlockingCollection<EthernetMessage> _dataItemsQueue2 = new BlockingCollection<EthernetMessage>(100); // max 100 items
        log4net.ILog _logger = StaticMethods.GetLogger();
        protected string _caseUrl; // remove?
        protected DeviceSetup _deviceSetup; // from config
        protected bool _isDeviceReady=false;
        protected OutputDevice(DeviceSetup deviceSetup)
        {
            _deviceSetup = deviceSetup;
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
        /// <summary>
        /// Message loop
        /// </summary>
        protected void OutputDeviceHandler_Task()
        {
            _logger.Debug($"OutputDeviceHandler_Task start. {InstanceName}");
            // message loop
            while (false == _dataItemsQueue2.IsCompleted)
            {
                EthernetMessage incomingMessage = _dataItemsQueue2.Take(); // get from q

                if (null == incomingMessage) // end task token
                {
                    continue;
                }

                // verify incoming message
                // slot number
                if (incomingMessage.SlotNumber != this._deviceSetup.SlotNumber)
                {
                    _logger.Warn($"{InstanceName} wrong device number. incoming message dropped.");
                }
                
                // device name

                if (_isDeviceReady)
                {
                    HandleRequest(incomingMessage);
                }
                else
                {
                    _logger.Warn($"Device {DeviceName} not ready. message dropped.");
                }
            }
            _logger.Debug($"OutputDeviceHandler_Task Fin. {InstanceName}");
        }

        public virtual void Dispose()
        {
            _dataItemsQueue2.Add(null); // end task token
            _dataItemsQueue2.CompleteAdding();
        }
    }
}
