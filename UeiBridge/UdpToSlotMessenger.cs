#define blocksim1
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using UeiBridge.Library;
using UeiBridge.Types;
using System.Collections.Concurrent;

namespace UeiBridge
{
    public class ConsumerEntry
    {
        public int SlotNumber { get; private set; }
        public int CubeId { get; private set; }
        public IEnqueue<byte[]> Consumer { get; private set; }

        public ConsumerEntry( int cubeId, int slotNumber,  IEnqueue<byte[]> consumer)
        {
            SlotNumber = slotNumber;
            CubeId = cubeId;
            Consumer = consumer;
        }
    }
    /// <summary>
    /// messenger
    /// 
    /// </summary>
    public class UdpToSlotMessenger : IEnqueue<byte[]>
    {
        private ILog _logger = StaticMethods.GetLogger();
        private List<ConsumerEntry> _consumersList = new List<ConsumerEntry>();
        private BlockingCollection<byte[]> _inputQueue = new BlockingCollection<byte[]>(1000); // max 1000 items

        public UdpToSlotMessenger()
        {
            Task.Factory.StartNew(() => DispatchToConsumer_Task());
        }
        public void Enqueue(byte[] byteMessage)
        {
            if (_inputQueue.IsCompleted)
            {
                return;
            }

            try
            {
                _inputQueue.Add(byteMessage);
            }
            catch (Exception ex)
            {
                _logger.Warn($"Incoming byte message error. {ex.Message}. message dropped.");
            }
        }

        void DispatchToConsumer_Task()
        {
            // message loop
            while (false == _inputQueue.IsCompleted)
            {
                byte[] incomingMessage = _inputQueue.Take(); // get from q

                if (null == incomingMessage) // end task token
                {
                    _inputQueue.CompleteAdding();
                    break;
                }

                EthernetMessage ethMag = EthernetMessage.CreateFromByteArray( incomingMessage, MessageWay.downstream);

                var clist = _consumersList.Where(consumer1 => ((consumer1.CubeId == ethMag.UnitId) && ( consumer1.SlotNumber == ethMag.SlotNumber )));
                if (clist.Count()==0) // no subs
                {
                    _logger.Warn($"No consumer to message aimed to slot {ethMag.SlotNumber} and unit id {ethMag.UnitId}");
                    continue;
                }
                if (clist.Count() > 1) // 2 subs with same parameters
                {
                    throw new ArgumentException();
                }

                ConsumerEntry consumer = clist.FirstOrDefault();
                consumer.Consumer.Enqueue(incomingMessage);
            }
        }

        public void SubscribeConsumer( IEnqueue<byte[]> consumer, int cubeId, int slotNumber)
        {
            ConsumerEntry ce = new ConsumerEntry(cubeId, slotNumber, consumer);
            _consumersList.Add(ce);
        }
    }
}
