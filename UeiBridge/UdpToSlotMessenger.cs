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
    public class UdpToSlotMessenger : IEnqueue<SendObject>
    {
        private ILog _logger = StaticMethods.GetLogger();
        private List<ConsumerEntry> _consumersList = new List<ConsumerEntry>();
        private BlockingCollection<SendObject> _inputQueue = new BlockingCollection<SendObject>(1000); // max 1000 items

        public UdpToSlotMessenger()
        {
            Task.Factory.StartNew(() => DispatchToConsumer_Task());
        }
        public void Enqueue(SendObject sendObject1)
        {
            if (_inputQueue.IsCompleted)
            {
                return;
            }

            try
            {
                _inputQueue.Add(sendObject1);
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
                SendObject incomingMessage = _inputQueue.Take(); // get from q

                if (null == incomingMessage) // end task token
                {
                    _inputQueue.CompleteAdding();
                    break;
                }

                string err=null;
                EthernetMessage ethMag = EthernetMessage.CreateFromByteArray( incomingMessage.ByteMessage, MessageWay.downstream,  ref err);

                if (null==ethMag)
                {
                    _logger.Warn($"Failed to parse incoming ethernet message (aimed to {incomingMessage.TargetEndPoint}). {err}");
                    continue;
                }

                var consumerList = _consumersList.Where(consumer1 => ((consumer1.CubeId == ethMag.UnitId) && ( consumer1.SlotNumber == ethMag.SlotNumber )));
                if (consumerList.Count()==0) // no subs
                {
                    _logger.Warn($"No consumer to message aimed to slot{ethMag.SlotNumber} /cube{ethMag.UnitId} ({incomingMessage.TargetEndPoint})");
                    continue;
                }

                foreach (ConsumerEntry ce in consumerList)
                {
                    ce.Consumer.Enqueue(incomingMessage.ByteMessage);
                }
            }
        }

        public void SubscribeConsumer( IEnqueue<byte[]> consumer, int cubeId, int slotNumber)
        {
            ConsumerEntry ce = new ConsumerEntry(cubeId, slotNumber, consumer);
            _consumersList.Add(ce);
        }
    }
}
