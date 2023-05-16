using System;
using System.Threading;
using System.Threading.Tasks;
using UeiDaq;
using UeiBridge.Library;
using UeiBridge.Types;

namespace UeiBridge
{
    /// <summary>
    /// SL508-United-Manager
    /// </summary>
    public class SL508UnitedManager : IDisposable, IEnqueue<byte[]>
    {
        private log4net.ILog _logger = StaticMethods.GetLogger();
        private DeviceEx realDevice;
        private DeviceSetup setup;
        private Session SrlSession;
        private SerialReader SrlReader;
        private SerialWriter SrlWriter;
        private AsyncCallback readerAsyncCallback;
        private IAsyncResult readerIAsyncResult;
        const int minLength = 200;

        public SL508UnitedManager(DeviceEx realDevice, DeviceSetup setup)
        {
            this.realDevice = realDevice;
            this.setup = setup;
        }

        internal void OpenDevice()
        {
            SrlSession = new Session();
            SerialPort p = SrlSession.CreateSerialPort("pdna://192.168.100.2/Dev3/Com0,1",
                                        SerialPortMode.RS232,
                                        SerialPortSpeed.BitsPerSecond19200,
                                        SerialPortDataBits.DataBits8,
                                        SerialPortParity.None,
                                        SerialPortStopBits.StopBits1,
                                        ""); ;

            // Configure timing to return serial message when either of the following conditions occurred
            // - The termination string was detected
            // - 100 bytes have been received
            // - 10ms elapsed (rate set to 100Hz);
            SrlSession.ConfigureTimingForMessagingIO(100, 100.0);
            SrlSession.GetTiming().SetTimeout(500);

            SrlReader = new SerialReader(SrlSession.GetDataStream(), SrlSession.GetChannel(0).GetIndex());
            SrlWriter = new SerialWriter(SrlSession.GetDataStream(), SrlSession.GetChannel(1).GetIndex());

            // Start the session
            SrlSession.Start();

            // Initiate one asynchronous read, it will re-initiate itself
            // automatically in the callback
            readerAsyncCallback = new AsyncCallback(ReaderCallback);
            readerIAsyncResult = SrlReader.BeginRead(minLength, readerAsyncCallback, null);

            foreach (Channel c in SrlSession.GetChannels())
            {
                SerialPort sp1 = c as SerialPort;
                _logger.Debug($"CH:{sp1.GetIndex()}, Speed:{sp1.GetSpeed()}, Mode: {sp1.GetMode()}");
            }

            return; 

            Task t = System.Threading.Tasks.Task.Factory.StartNew(() => 
            { 
                for(int i =0; i<50; i++)
                {
                    Thread.Sleep(10);
                    byte b = Convert.ToByte(i);
                    SrlWriter.Write(new byte[] { b, 02, 03 });
                    
                }
            });
        }

        private void ReaderCallback(IAsyncResult ar)
        {
            try
            {
                byte[] recvBytes = SrlReader.EndRead(ar);

                // We can't directly access the UI from an asynchronous method
                // need to invoke a delegate that will take care of updating
                // the UI from the proper thread
                if (recvBytes != null)
                {
                    _logger.Debug($"first byte {recvBytes[0]}, length {recvBytes.Length}");
                }

                if (SrlSession != null && SrlSession.IsRunning())
                {
                    readerIAsyncResult = SrlReader.BeginRead(minLength, readerAsyncCallback, null);
                }
            }
            catch (UeiDaqException ex)
            {
                // only handle exception if the session is running
                if (SrlSession.IsRunning())
                {
                    if (Error.Timeout == ex.Error)
                    {
                        // Ignore timeout error, they will occur if the send button is not
                        // clicked on fast enough!
                        // Just re-initiate a new asynchronous read.
                        readerIAsyncResult = SrlReader.BeginRead(minLength, readerAsyncCallback, null);
                        //_logger.Debug("Timeout");
                    }
                    else
                    {
                        SrlSession.Dispose();
                        SrlSession = null;
                        _logger.Warn($"UeiDaqException. session disposed. {ex.Message}");
                    }
                }
                else
                {
                    _logger.Warn($"UeiDaqException. not running. {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                _logger.Warn($"UeiDaqException. {ex.Message}");
            }
        }

        public void Dispose()
        {
            try
            {
                SrlSession.Stop();
            }
            catch (UeiDaqException ex)
            {
                _logger.Warn($"UeiDaqException. {ex.Message}");
            }

            // wait for current async call to complete
            // before destroying the session
            readerIAsyncResult.AsyncWaitHandle.WaitOne();

            SrlSession.Dispose();
            SrlSession = null;
            _logger.Debug("Session disposed");
        }

        public void Enqueue(byte[] byteMessage)
        {
            SrlWriter.Write( byteMessage);
            //SrlWriter.Write(new byte[] { 01, 02, 03 });
        }
    }
}
