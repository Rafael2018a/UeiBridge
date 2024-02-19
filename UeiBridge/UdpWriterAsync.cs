using System;
using System.Threading.Tasks;
using System.Net;
using UeiBridge.Library.Interfaces;
using UeiBridge.Library;
using System.Collections.Concurrent;

namespace UeiBridge
{
    /// <summary>
    /// Decorator of UdpWriter
    /// Add async capability. For serial card (SL508) only
    /// </summary>
    public class UdpWriterAsync : IEnqueue<SendObject2>, IDisposable
    {
        UdpWriter _uWriter; // tbd. use UdpWriter2
        private BlockingCollection<SendObject2> _dataItemsQueue2 = new BlockingCollection<SendObject2>(100); // max 100 items
        public UdpWriterAsync(IPEndPoint destEp, string localBindAddress)
        {
            _uWriter = new UdpWriter(destEp, localBindAddress);
            Task.Run(() => Task_SendMessageLoop());
        }
        public void Dispose()
        {
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
        protected void Task_SendMessageLoop()
        {

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

                    byte[] buf = so2.MessageBuild(so2.RawByteMessage);
                    SendObject so = new SendObject(so2.TargetEndPoint, buf);
                    _uWriter.Send(so);
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
        }
    }

}
