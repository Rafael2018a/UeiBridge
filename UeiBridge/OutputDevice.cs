﻿using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using UeiDaq;
using UeiBridge.Types;
using UeiBridge.Library;

namespace UeiBridge
{
    /// <summary>
    /// Parent class for all [x]outputDeviceManger classes.
    /// </summary>
    public abstract class OutputDevice : IDeviceManager,  IDisposable, IEnqueue<byte[]> // IEnqueue<DeviceRequest>,
    {
        public abstract string DeviceName { get; }
        public abstract bool OpenDevice();
        public abstract string[] GetFormattedStatus(TimeSpan interval);
        protected abstract void HandleRequest(EthernetMessage request);

        public string InstanceName { get; private set; }
        public int SlotNumber { get; private set; }
        //protected DeviceSetup _deviceSetup; 
        protected bool _isDeviceReady=false;
       
        private BlockingCollection<EthernetMessage> _dataItemsQueue2 = new BlockingCollection<EthernetMessage>(100); // max 100 items
        private log4net.ILog _logger = StaticMethods.GetLogger();

        protected OutputDevice() { }
        protected OutputDevice(DeviceSetup deviceSetup)
        {
            if (null != deviceSetup)
            {
                InstanceName = $"{DeviceName}/Cube{deviceSetup.CubeId}/Slot{deviceSetup.SlotNumber}/Output";
            }
            else
            {
                InstanceName = "<undefiend output device>";
                throw new ArgumentNullException();
            }
            this.SlotNumber = deviceSetup.SlotNumber;
        }

        /// <summary>
        /// Change byte-message to 'EthernetMessage' and push to message-loop-queue
        /// </summary>
        public virtual void Enqueue(byte[] m)
        {
            while (_dataItemsQueue2.IsCompleted)
            {
                return;
            }

            try
            {
                EthernetMessage em = EthernetMessage.CreateFromByteArray(m, MessageWay.downstream);
                System.Diagnostics.Debug.Assert(em != null);
                if (!_dataItemsQueue2.IsCompleted)
                {
                    _dataItemsQueue2.Add(em);
                }
            }
            catch (Exception ex)
            {
                _logger.Warn($"Incoming byte message error. {ex.Message}. message dropped.");
            }

        }
        /// <summary>
        /// Message loop
        /// </summary>
        protected void OutputDeviceHandler_Task()
        {
            // message loop
            while (false == _dataItemsQueue2.IsCompleted)
            {
                EthernetMessage incomingMessage = _dataItemsQueue2.Take(); // get from q

                if (null == incomingMessage) // end task token
                {
                    _dataItemsQueue2.CompleteAdding();
                    break;
                }

                // verify internal consistency
                if (false == incomingMessage.InternalValidityTest())
                {
                    _logger.Warn("Invalid message. rejected");
                    continue;
                }
                // verify valid card type
                int cardId = DeviceMap2.GetCardIdFromCardName(this.DeviceName);
                if ( cardId != incomingMessage.CardType)
                {
                    _logger.Warn($"{InstanceName} wrong card id {incomingMessage.CardType} while expecting {cardId}. message dropped.");
                    continue;
                }
                // verify slot number
                if (incomingMessage.SlotNumber != this.SlotNumber)
                {
                    _logger.Warn($"{InstanceName} wrong slot number ({incomingMessage.SlotNumber}). incoming message dropped.");
                    continue;
                }
                // alert if items lost
                if (_dataItemsQueue2.Count ==  _dataItemsQueue2.BoundedCapacity)
                {
                    _logger.Warn($"Input queue items = {_dataItemsQueue2.Count}");
                }

                // finally, Handle message
                if (_isDeviceReady)
                {
                    HandleRequest(incomingMessage);
                }
                else
                {
                    _logger.Warn($"Device {DeviceName} not ready. message rejected.");
                }
            }
        }

        public static void CloseSession( Session theSession)
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

        public virtual void Dispose()
        {
            _logger.Debug($"Device manager {InstanceName} Disposed");
        }
        public virtual void HaltMessageLoop()
        {
            _dataItemsQueue2.Add(null); // end task token (first, free Take() api and then apply CompleteAdding()
            Thread.Sleep(100);
            _dataItemsQueue2.CompleteAdding();
        }
    }
}
