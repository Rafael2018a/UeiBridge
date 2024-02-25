using System;
using System.Threading.Tasks;
using System.Net;
using UeiBridge.Library.Interfaces;
using UeiBridge.Library;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Timers;

namespace UeiBridge
{
    /// <summary>
    /// Decorator of UdpWriter
    /// Add async capability. For serial card (SL508) only
    /// </summary>
    public class UdpWriterAsync : IEnqueue<SendObject2>, IDisposable
    {
        ISend<SendObject> _uWriter; // tbd. use UdpWriter2
        string _instanceName=null;
        readonly UInt16 _messagePreamble;
        private BlockingCollection<SendObject2> _dataItemsQueue2 = new BlockingCollection<SendObject2>(100); // max 100 items
        private log4net.ILog _logger = StaticLocalMethods.GetLogger();
        List<long> _knownLengthList=new List<long>();
        public Dictionary<int, int> _lengthCountList = new Dictionary<int, int>(); // (msgLen, CountOfMsgLen)
        System.Timers.Timer _tm = new System.Timers.Timer();
        public UdpWriterAsync( ISend<SendObject> uwriter, UInt16 messagePreamble)
        {
            _uWriter = uwriter;
            _messagePreamble = messagePreamble;
        }
        public void SetInstanceName(string instanceName)
        {
            _instanceName = instanceName;
        }
        public void Start()
        {
            Task.Run(() => Task_SendMessageLoop(_instanceName));

            _tm.AutoReset = true;
            _tm.Interval = 500;
            _tm.Elapsed += TimerCallback;
            _tm.Start();

        }
        public void Dispose()
        {
            _dataItemsQueue2.CompleteAdding();
            _uWriter.Dispose();
        }

        public void Enqueue(SendObject2 so2)
        {
            if (_dataItemsQueue2.IsCompleted)
            {
                return;
            }
            _dataItemsQueue2.Add(so2);
        }
        protected void Task_SendMessageLoop(string callerInstanceName)
        {

            System.Threading.Thread.CurrentThread.Name = "Task:UdpWriterAsync:" + callerInstanceName;
           
            _logger.Debug($"{System.Threading.Thread.CurrentThread.Name} start");
            // message loop
            // ============
            while (false == _dataItemsQueue2.IsCompleted)
            {
                try
                {
                    SendObject2 so2 = _dataItemsQueue2.Take(); // get from q

                    if (null == so2) // end task token
                    {
                        _dataItemsQueue2.CompleteAdding();
                        break;
                    }

                    List<byte[]> msgs;
                    bool isValidMessage = ScanMessage(so2.RawByteMessage, out msgs);

                    foreach (byte[] m in msgs)
                    {
                        // send to consumer
                        byte[] buf = so2.MessageBuilder( m);
                        SendObject so = new SendObject(so2.TargetEndPoint, buf);
                        _uWriter.Send(so);

                        // add message length to lengthCountList
                        if (isValidMessage)
                        {
                            if (_lengthCountList.ContainsKey( m.Length))
                            {
                                ++_lengthCountList[ m.Length];
                            }
                            else
                            {
                                _lengthCountList.Add( m.Length, 1);
                            }
                        }
                    }
                }
                catch (InvalidOperationException ex) // thrown if _downstreamQueue marked as complete
                {
                    // nothing to do here
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
            _logger.Debug($"{System.Threading.Thread.CurrentThread.Name} end");
        }

        private bool ScanMessage(byte[] rawByteMessage, out List<byte[]> msgs)
        {
            bool rc = false;
            if (rawByteMessage.Length<2)
            {
                msgs = new List<byte[]>(); // return empty list
                goto exit;
            }
            UInt16 first = BitConverter.ToUInt16(rawByteMessage, 0);
            if (first == _messagePreamble)
            {
                rc = true;
                // need to scan?
                if (_knownLengthList.Contains(rawByteMessage.Length)) // known length, no need to scan
                {
                    rc = true;
                }
                else // scan, try to find preamles within message
                {
                    _logger.Debug($"Scanning message. len: {rawByteMessage.Length}");
                    for(int j=2; j<rawByteMessage.Length-1; ++j)
                    {
                        UInt16 short1 = BitConverter.ToUInt16(rawByteMessage, j);
                        if (_messagePreamble==short1)
                        {
                            byte[] part1 = new byte[j];
                            byte[] part2 = new byte[rawByteMessage.Length - j];
                            Array.Copy(rawByteMessage, part1, j);
                            Array.Copy(rawByteMessage, j, part2, 0, part2.Length);
                            msgs = new List<byte[]> { part1, part2 };
                            goto exit;
                        }
                    }
                }
            }
            msgs = new List<byte[]> { rawByteMessage };

            exit: return rc;
        }

        public void TimerCallback(object sender, ElapsedEventArgs e)
        {
            // decay process
            for (int i = 0; i < _lengthCountList.Count; ++i)
            {
                if (_lengthCountList[i] > 0)
                {
                    --_lengthCountList[i];
                }
            }

            // update _knownLengthList
            foreach( var entry in _lengthCountList)
            {
                if (entry.Value > 20)
                {
                    if (!_knownLengthList.Contains(entry.Key))
                    {
                        _knownLengthList.Add(entry.Key);
                    }
                }

                if (entry.Value < 10)
                {
                    if (!_knownLengthList.Contains(entry.Key))
                    {
                        _knownLengthList.Remove(entry.Key);
                    }
                }
            }
        }
        /// <summary>
        /// rules:
        /// If not starts with sync bytes, drop.
        /// if there are sync bytes at offset > 2, split
        /// </summary>
        /// <param name="rawByteMessage"></param>
        /// <returns></returns>
        private List<byte[]> ScanMessage1(byte[] rawByteMessage, UInt16 preamble)
        {
            if (BitConverter.ToUInt16(rawByteMessage, 0)!=preamble)
            {
                return new List<byte[]>(); // empty list
            }
            return null;
            //Array.Find( rawByteMessage, 3, rawByteMessage.Length-1, (v => )
                //FindIndex<T>(T[] array, int startIndex, int count, Predicate < T > match);
        }
    }

}
