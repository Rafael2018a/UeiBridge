using System;
using UeiBridge.Library.Interfaces;
using UeiBridge.Library;
//using UeiBridge.Library.Types;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace SerialOp
{
    /// <summary>
    /// This class contains methods and fields which are common to all device managers
    /// </summary>
    public abstract class DeviceManagerBase : IDeviceManager
    {
        // publics
        public string DeviceName { get; protected set; }
        public string InstanceName { get; protected set; }
        // protected
        protected int _deviceSlotIndex;
        protected bool _isOutputDeviceReady = true;
        protected bool _inDisposeFlag = false;
        protected CancellationTokenSource _cancelTokenSource = new CancellationTokenSource();
        protected Task _downstreamTask;
        abstract protected void HandleDownstreamRequest(EthernetMessage request);
        // privates
        private BlockingCollection<EthernetMessage> _downstreamQueue = new BlockingCollection<EthernetMessage>(100); // max 100 items
        private Action<string> _act = new Action<string>(s => Console.WriteLine($"Failed to parse downstream message. {s}"));

        public void TerminateDownstreamTask()
        {
            _cancelTokenSource.Cancel();
            _downstreamQueue.CompleteAdding();
        }
        /// <summary>
        /// Translate and enqueue downstream message
        /// </summary>
        public void Enqueue(byte[] message)
        {
            if ((_downstreamQueue.IsCompleted)||(_isOutputDeviceReady==false))
            {
                return;
            }

            try
            {
                EthernetMessage em = EthernetMessage.CreateFromByteArray(message, MessageWay.downstream, _act);
                if (null == em)
                {
                    return;
                }

                if (false == _downstreamQueue.TryAdd(em))
                {
                    Console.WriteLine($"Incoming message dropped due to full message queue");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Incoming message dropped. {ex.Message}.");
            }

        }

        protected void DownstreamMessageLoop_Task()
        {
            // message loop
            // ============
            while ( _cancelTokenSource.Token.IsCancellationRequested==false)
            {
                try
                {
                    EthernetMessage incomingMessage = _downstreamQueue.Take(); 

                    // verify internal consistency
                    if (false == incomingMessage.InternalValidityTest())
                    {
                        Console.WriteLine("Invalid message. rejected");
                        continue;
                    }
                    // verify valid card type
                    int cardId = DeviceMap2.GetDeviceIdFromName(this.DeviceName);
                    if (cardId != incomingMessage.CardType)
                    {
                        Console.WriteLine($"{InstanceName} wrong card id {incomingMessage.CardType} while expecting {cardId}. message dropped.");
                        continue;
                    }
                    // verify slot number
                    if (incomingMessage.SlotNumber != this._deviceSlotIndex)
                    {
                        Console.WriteLine($"{InstanceName} wrong slot number ({incomingMessage.SlotNumber}). incoming message dropped.");
                        continue;
                    }
                    // alert if items lost
                    if (_downstreamQueue.Count == _downstreamQueue.BoundedCapacity)
                    {
                        Console.WriteLine($"Input queue items = {_downstreamQueue.Count}");
                    }

                    // finally, Handle message
                    if (_isOutputDeviceReady)
                    {
                        HandleDownstreamRequest(incomingMessage);
                    }
                    else
                    {
                        Console.WriteLine($"Device {DeviceName} not ready. message rejected.");
                    }
                }
                catch(InvalidOperationException ex) // _downstreamQueue marked as complete (for task termination)
                {
                    //Console.WriteLine(ex.Message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            _downstreamQueue.CompleteAdding();
        }

        public abstract string[] GetFormattedStatus(TimeSpan interval);

        public abstract void Dispose();
    }
}
