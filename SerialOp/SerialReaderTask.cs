using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UeiDaq;

namespace SerialOp
{
    internal class SerialReaderTask
    {
        private Session _ueiSession;
        List<SerialReader> _readerList;
        const int recvSize = 14;

        public SerialReaderTask(Session theSession)
        {
            _ueiSession = theSession;

            // Configure writer and reader for each channel
            var readers = new SerialReader[theSession.GetNumberOfChannels()];
            _readerList = new List<SerialReader>(readers);

            for (int chNum = 0; chNum < theSession.GetNumberOfChannels(); chNum++)
            {
                int chanNum = theSession.GetChannel(chNum).GetIndex();

                _readerList[chNum] = new SerialReader(theSession.GetDataStream(), chanNum);
            }
        }

        internal Task Start()
        {
            Task taskA = new Task(() =>
            {
                while (true)
                {
                    for (int i = 0; i < _ueiSession.GetNumberOfChannels(); i++)
                    {
                        int chanNum = _ueiSession.GetChannel(i).GetIndex();
                        //Console.WriteLine("reader{i}: ");

                        // "available" is the amount of data remaining from a single event
                        // more data may be in the queue once "available" bytes have been read
                        int available;
                        while ((available = _ueiSession.GetDataStream().GetAvailableInputMessages(chanNum)) > 0 && recvSize > 0)
                        {
                            Console.Write("  avail: {0}", _ueiSession.GetDataStream().GetAvailableInputMessages(chanNum));
                            try
                            {
                                byte[] buf = _readerList[i].Read(recvSize);
                                string readString = Encoding.ASCII.GetString(buf);
                                Console.WriteLine($" Read data: { readString}");
                            }
                            catch (UeiDaqException e)
                            {
                                if (e.Error == Error.Timeout)
                                {
                                    Console.WriteLine("  read timeout");
                                    break;
                                }
                                else
                                {
                                    throw e;
                                }
                            }

                            System.Threading.Thread.Sleep(100);
                        }
                    }

                }
            });

            // Start the task.
            taskA.Start();

            return taskA;
        }
    }
}