﻿using System;
using System.Collections.Concurrent;
using System.Threading;
using UeiBridge.Library;
using UeiBridge.Library.CubeSetupTypes;
using UeiBridge.Library.Interfaces;
using UeiDaq;

namespace UeiBridge
{
    /// <summary>
    /// Parent class for all [x]outputDeviceManger classes.
    /// </summary>
    public abstract class OutputDevice : IDeviceManager, IDisposable, IEnqueue<byte[]> // IEnqueue<DeviceRequest>,
    {
        public abstract string DeviceName { get; }
        public abstract bool OpenDevice();
        public abstract string[] GetFormattedStatus(TimeSpan interval);
        protected abstract void HandleRequest(EthernetMessage request);

        public string InstanceName { get; private set; }
        //public int SlotNumber { get; private set; }
        //public string CubeUrl { get; private set; }
        //public int CubeId { get; private set; }
        public UeiDeviceInfo DeviceInfo { get; private set; }
        
        protected bool _isDeviceReady = false;
        protected bool _inDisposeFlag = false;
        private BlockingCollection<EthernetMessage> _dataItemsQueue2 = new BlockingCollection<EthernetMessage>(100); // max 100 items
        private log4net.ILog _logger = StaticLocalMethods.GetLogger();
        protected ISession _iSession;

        protected OutputDevice() { }
        protected OutputDevice(DeviceSetup deviceSetup)
        {
            InstanceName = deviceSetup.GetInstanceName() + "/Output";
            //this.SlotNumber = deviceSetup.SlotNumber;
            //this.CubeUrl = deviceSetup.CubeUrl;

            //IPAddress ipa = Config2.CubeUriToIpAddress(this.CubeUrl);
            //if (null != ipa)
            //{
            //    CubeId = ipa.GetAddressBytes()[3];
            //}
            //else
            //{
            //    CubeId = -1;
            //}

            DeviceInfo = deviceSetup.GetDeviceInfo();
        }

        /// <summary>
        /// Change byte-message to 'EthernetMessage' and push to message-loop-queue
        /// </summary>
        public virtual void Enqueue(byte[] m)
        {
            if (_dataItemsQueue2.IsCompleted)
            {
                return;
            }

            try
            {
                EthernetMessage em = EthernetMessage.CreateFromByteArray(m, MessageWay.downstream, new Action<string>(s => _logger.Warn(s)));
                if (null==em)
                {
                    return;
                }
                
                _dataItemsQueue2.Add(em);
            }
            catch (Exception ex)
            {
                _logger.Warn($"Incoming byte message error. {ex.Message}. message dropped.");
            }

        }
        /// <summary>
        /// Output device message loop
        /// This method is common for all device types, except
        /// HandleRequest() method which is unique per device.
        /// </summary>
        protected void Task_OutputDeviceHandler()
        {
            //var st = new System.Diagnostics.StackTrace();
            System.Threading.Thread.CurrentThread.Name = "Task:OutDeviceHandler:" + InstanceName; //st.GetFrame(1).GetMethod().DeclaringType.Name;
            _logger.Debug($"{System.Threading.Thread.CurrentThread.Name} start");
            // message loop
            // ============
            while (false == _dataItemsQueue2.IsCompleted)
            {
                try
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
                    int cardId = DeviceMap2.GetDeviceIdFromName(this.DeviceName);
                    if (cardId != incomingMessage.CardType)
                    {
                        _logger.Warn($"{InstanceName} wrong card id {incomingMessage.CardType} while expecting {cardId}. message dropped.");
                        continue;
                    }
                    // verify slot number
                    if (incomingMessage.SlotNumber != this.DeviceInfo.DeviceSlot)
                    {
                        _logger.Warn($"{InstanceName} wrong slot number ({incomingMessage.SlotNumber}). incoming message dropped.");
                        continue;
                    }
                    // alert if items lost
                    if (_dataItemsQueue2.Count == _dataItemsQueue2.BoundedCapacity)
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
                catch( Exception ex)
                {
                    _logger.Warn(ex.Message);
                }
            }
            _logger.Debug($"{System.Threading.Thread.CurrentThread.Name} end");
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
        //    HaltMessageLoop();
        //    _logger.Debug($"Device manager {InstanceName} Disposed");
        //}
        public virtual void TerminateMessageLoop()
        {
            _dataItemsQueue2.Add(null); // end task token (first, free Take() api and then apply CompleteAdding()
            Thread.Sleep(100);
            _dataItemsQueue2.CompleteAdding();
        }

        protected void EmitInitMessage( string deviceMessage)
        {
            _logger.Info($"Cube{DeviceInfo.CubeId}/Slot{DeviceInfo.DeviceSlot}: {deviceMessage}");
        }
    }
}
