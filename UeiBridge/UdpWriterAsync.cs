#define joinmessages
using System;
using System.Threading.Tasks;
using System.Net;
using UeiBridge.Library.Interfaces;
using UeiBridge.Library;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Timers;
using System.Text;

namespace UeiBridge
{
    /// <summary>
    /// Decorator of UdpWriter
    /// Add async capability. For serial card (SL508) only
    /// </summary>
    public class UdpWriterAsync : IEnqueue<SendObject2>, IDisposable
    {
        ISend<SendObject> _uWriter; // tbd. use UdpWriter2
        string _instanceName = null;
        readonly UInt16 _messagePreamble;
        private BlockingCollection<SendObject2> _dataItemsQueue2 = new BlockingCollection<SendObject2>(100); // max 100 items
        private log4net.ILog _logger = StaticLocalMethods.GetLogger();
        List<long> _knownLengthList = new List<long>();
        public Dictionary<int, int> _lengthCountList = new Dictionary<int, int>(); // (msgLen, CountOfMsgLen)
        System.Timers.Timer _tm = new System.Timers.Timer();
        public UdpWriterAsync(ISend<SendObject> uwriter, UInt16 messagePreamble)
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
            if (false == _dataItemsQueue2.TryAdd(so2))
            {
                _logger.Warn("Failed to add message");
            }
        }
        protected void Task_SendMessageLoop(string callerInstanceName)
        {

            System.Threading.Thread.CurrentThread.Name = "Task:UdpWriterAsync:" + callerInstanceName;
            _logger.Debug($"{System.Threading.Thread.CurrentThread.Name} start");

            int internalCounter = 0;
            int p35 = 0;
            
            
            byte[] pattern = BitConverter.GetBytes(_messagePreamble);

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

                    

                    if ((132 == so2.RawByteMessage.Length) || ((35 == so2.RawByteMessage.Length)))
                    {
                        byte[] fullmessage = so2.MessageBuilder(so2.RawByteMessage);
                        SendObject so = new SendObject(so2.TargetEndPoint, fullmessage);
                        _uWriter.Send(so);
                        continue;
                    }

#if joinmessages
                    _logger.Debug($"scanning upstream message. Length {so2.RawByteMessage.Length}");
                    int startIndex = 0;
                    int indexof;
                    while ((indexof = StaticMethods.IndexOf(so2.RawByteMessage, pattern, startIndex + pattern.Length)) != -1)
                    {
                        byte[] dest = new byte[indexof - startIndex];
                        Array.Copy(so2.RawByteMessage, startIndex, dest, 0, dest.Length);
                        byte[] fullmessage = so2.MessageBuilder(dest);
                        SendObject so = new SendObject(so2.TargetEndPoint, fullmessage);
                        _uWriter.Send(so);
                        _logger.Debug($"sent after scan: {dest.Length}");

                        startIndex = indexof;
                    }



                    // add message length to lengthCountList
                    //if (isWholeMessage)
                    //{
                    //    int len = so2.RawByteMessage.Length;
                    //    lock (_lengthCountList)
                    //    {
                    //        if (_lengthCountList.ContainsKey(len))
                    //        {
                    //            ++_lengthCountList[len];
                    //        }
                    //        else
                    //        {
                    //            _lengthCountList.Add(len, 1);
                    //        }
                    //    }
                    //}


#else
                    byte[] buf = so2.MessageBuilder(so2.RawByteMessage);
                    SendObject so = new SendObject(so2.TargetEndPoint, buf);
                    _uWriter.Send(so);
                    if ((132 != so2.RawByteMessage.Length) && ((60 != so2.RawByteMessage.Length)) && ((35 != so2.RawByteMessage.Length)))
                    {
                        _logger.Warn($"unknown upstream message. Length {so2.RawByteMessage.Length}");
                    }
                    //else
                    {
                        //uint timestampMs = BitConverter.ToUInt32(so2.RawByteMessage, 0)/1000;
                        //_logger.Debug($"timestampMs  upstream message. Length {so2.RawByteMessage.Length}");
                    }


#endif

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

        private void ScanMessage(byte[] rawByteMessage, out List<byte[]> partialMessageList, out bool isWholeMessage)
        {
            isWholeMessage = false;
            //bool isvalidmessage = false;
            if (rawByteMessage.Length < 2)
            {
                partialMessageList = new List<byte[]>(); // return empty list
                return;
            }

            UInt16 first = BitConverter.ToUInt16(rawByteMessage, 0);
            if (first == _messagePreamble)
            {
                // need to scan?
                if (_knownLengthList.Contains(rawByteMessage.Length)) // known length, no need to scan
                {
                    partialMessageList = new List<byte[]> { rawByteMessage };
                }
                else // scan, try to split message
                {
                    if (rawByteMessage.Length == 132)
                    {
                    }

                    partialMessageList = new List<byte[]>();
                    int p1 = 0;
                    for (int p2 = 2; p2 < rawByteMessage.Length - 1; ++p2)
                    {
                        UInt16 short1 = BitConverter.ToUInt16(rawByteMessage, p2);
                        if (_messagePreamble == short1)
                        {
                            byte[] part1 = new byte[p2 - p1];
                            Array.Copy(rawByteMessage, p1, part1, 0, p2 - p1);
                            p1 = p2;
                            partialMessageList.Add(part1);
                        }
                    }

                    // if preamble was found once (in middle of message) complete last part
                    if (0 != p1)
                    {
                        int p2 = rawByteMessage.Length;
                        byte[] part1 = new byte[p2 - p1];
                        Array.Copy(rawByteMessage, p1, part1, 0, p2 - p1);
                        p1 = p2;
                        partialMessageList.Add(part1);
                    }
                    else
                    {
                        partialMessageList.Add(rawByteMessage);
                        isWholeMessage = true;
                    }

                    // for log only
                    if (partialMessageList.Count > 1)
                    {
                        StringBuilder sb = new StringBuilder("Message split ");
                        //Console.Write("Message split ");
                        foreach (var ent in partialMessageList)
                        {
                            sb.Append($"{ent.Length} - ");
                        }
                        _logger.Debug(sb.ToString());
                    }
                }

            }
            else
            {
                partialMessageList = new List<byte[]>(); // return empty list
            }
        }

        public void TimerCallback(object sender, ElapsedEventArgs e)
        {
            // decay process
            //for (int i = 0; i < _lengthCountList.Count; ++i)
            //{
            //    if (_lengthCountList[i] > 0)
            //    {
            //        --_lengthCountList[i];
            //    }
            //}

            // update _knownLengthList
            lock (_lengthCountList)
            {
                foreach (var entry in _lengthCountList)
                {
                    if (entry.Value > 20)
                    {
                        if (false == _knownLengthList.Contains(entry.Key))
                        {
                            _knownLengthList.Add(entry.Key);
                            _logger.Debug($"Key {entry.Key} added");
                        }
                    }

                    //if (entry.Value < 10)
                    //{
                    //    if (_knownLengthList.Contains(entry.Key))
                    //    {
                    //        _knownLengthList.Remove(entry.Key);
                    //        _logger.Debug($"Key {entry.Key} removed");
                    //    }
                    //}
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
            if (BitConverter.ToUInt16(rawByteMessage, 0) != preamble)
            {
                return new List<byte[]>(); // empty list
            }
            return null;
            //Array.Find( rawByteMessage, 3, rawByteMessage.Length-1, (v => )
            //FindIndex<T>(T[] array, int startIndex, int count, Predicate < T > match);
        }
    }

}
