using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using UeiDaq;
using UeiBridgeTypes;

/// <summary>
/// All files in project might refer to this file.
/// Types in this file might NOT refer to types in any other file.
/// </summary>
namespace UeiBridge
{
    public abstract class OutputDevice : IDeviceManager,  IDisposable, IEnqueue<byte[]> // IEnqueue<DeviceRequest>,
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
        
        // protected fields
        // ----------------
        protected string _caseUrl; // remove?
        protected DeviceSetup _deviceSetup; // from config
        protected bool _isDeviceReady=false;

        // privates
        // ---------
        BlockingCollection<EthernetMessage> _dataItemsQueue2 = new BlockingCollection<EthernetMessage>(100); // max 100 items
        log4net.ILog _logger = StaticMethods.GetLogger();
        System.Timers.Timer _resetLastScanTimer = new System.Timers.Timer(1000);


        protected OutputDevice(DeviceSetup deviceSetup)
        {
            _deviceSetup = deviceSetup;
            _resetLastScanTimer.Elapsed += resetLastScanTimer_Elapsed;
            _resetLastScanTimer.AutoReset = true;
            _resetLastScanTimer.Enabled = true;
        }

        protected abstract void resetLastScanTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e);

        public void Enqueue(byte[] m)
        {
            string errorString;
            EthernetMessage em = EthernetMessage.CreateFromByteArray(m, out errorString);
            if (em != null)
            {
                if (!_dataItemsQueue2.IsCompleted)
                {
                    _dataItemsQueue2.Add(em);
                }
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
                    break;
                }

                // verify card type
                int cardId = StaticMethods.GetCardIdFromCardName(this.DeviceName);
                if ( cardId != incomingMessage.CardType)
                {
                    _logger.Warn($"{InstanceName} wrong card id {incomingMessage.CardType} while expecting {cardId}. message dropped.");
                    continue;
                }
                // verify payload length
                // tbd

                // verify slot number
                if (incomingMessage.SlotNumber != this._deviceSetup.SlotNumber)
                {
                    _logger.Warn($"{InstanceName} wrong slot number. incoming message dropped.");
                    continue;
                }
                
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
            Thread.Sleep(100);
            _dataItemsQueue2.CompleteAdding();
        }
    }
}
