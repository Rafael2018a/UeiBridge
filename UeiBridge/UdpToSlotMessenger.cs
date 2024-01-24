﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using UeiBridge.Library;
using UeiBridge.Library.Types;
using System.Collections.Concurrent;
using UeiBridge.Library.Interfaces;

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
        private ILog _logger = StaticLocalMethods.GetLogger();
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

                EthernetMessage ethMag = EthernetMessage.CreateFromByteArray( incomingMessage.ByteMessage, MessageWay.downstream, new Action<string>(s => _logger.Warn(s)));
                if (null==ethMag)
                {
                    _logger.Warn($"Failed to parse incoming Ethernet message (aimed to {incomingMessage.TargetEndPoint}).");
                    continue;
                }

                var consumerList = _consumersList.Where(consumer1 => ((consumer1.CubeId == ethMag.CubeId) && ( consumer1.SlotNumber == ethMag.SlotNumber )));
                if (consumerList.Count()==0) // no subs
                {
                    _logger.Warn($"No consumer to message aimed to slot{ethMag.SlotNumber} /cube{ethMag.CubeId} ({incomingMessage.TargetEndPoint})");
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
