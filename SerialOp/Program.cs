using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using UeiBridge.Library;
using UeiBridge.Library.CubeSetupTypes;
using UeiDaq;

namespace SerialOp
{

    /// <summary>
    /// serial-agent -r -ch 1
    /// </summary>
    public class Program
    {
        //private Session _serialSession;
        //List<ChannelAux> _channelAuxList;
        CubeSetup _cubeSetup;
        bool stopByUser = false;
        bool stopByWatchdog = false;


        static void Main()
        {
            Program p = new Program();
            p.MainSerial();
        }


        public void MainSerial()
        {
            // register CTRL + c handler
            Console.CancelKeyPress += new ConsoleCancelEventHandler(CancelEventHandler);

            // load setting
            FileInfo setupfile = new FileInfo("Cube3.config");
            var csl = new CubeSetupLoader(setupfile);
            if (null == csl.CubeSetupMain)
            {
                Console.WriteLine($"File to load setup file {setupfile.FullName}");
                return;
            }
            _cubeSetup = csl.CubeSetupMain;


            int deviceSlotIndex = 0; // slot index in given cube
            var deviceSetup = _cubeSetup.GetDeviceSetupEntry( deviceSlotIndex) as SL508892Setup;

            do // watchdog loop
            {
                // set session
                // -----------
                Session serSession = BuildSerialSession( deviceSetup);
                if (null==serSession)
                {
                    break;// loop
                }
                serSession.Start();

                Console.WriteLine($"Listening on {serSession.GetDevice().GetResourceName()}");

                // set device manager and watchdog
                // -------------------------------
                SL508DeviceManager deviceManager = new SL508DeviceManager(null, deviceSetup, serSession);
                SerialWatchdog swd = new SerialWatchdog(new Action<string>((i) => { stopByWatchdog = true; }));
                deviceManager.SetWatchdog(swd);
                deviceManager.OpenDevice();

                // sleep
                // -----
                Console.WriteLine("^C to stop");
                int seed = 10;
                do { 
                    System.Threading.Thread.Sleep(1000);
                    //List<byte[]> msgs = StaticMethods.Make_SL508Down_Messages(++seed);
                    //foreach (byte[] m in msgs)
                    //{
                    //    deviceManager.Enqueue(m); //new byte[] { 0, 1, 2 });
                    //}
                } while (false == stopByUser && false == stopByWatchdog);

                Console.WriteLine("Serial stat\n--------");
                foreach (var ch in deviceManager.ChannelStatList)
                {
                    Console.WriteLine($"CH {ch.ChannelIndex}: {ch.ToString()}");
                }
                // dispose process
                // ----------------
                deviceManager.Dispose();
                deviceManager = null;

                serSession.Stop();
                System.Diagnostics.Debug.Assert(false == serSession.IsRunning());
                serSession.Dispose();
                serSession = null;

                Console.WriteLine(" = Dispose fin =");

                // wait before restart
                if (true==stopByWatchdog)
                {
                    System.Threading.Thread.Sleep(1000);
                    stopByWatchdog = false;
                }
                swd = null;
            } while (false == stopByUser);

            Console.WriteLine("Any key to exit...");
            Console.ReadKey();
        }

        bool DeviceReset_old(string deviceUri)
        {
            Device myDevice = DeviceEnumerator.GetDeviceFromResource(deviceUri);

            if (null != myDevice)
            {
                //string devName = myDevice.GetDeviceName();
                //System.Diagnostics.Debug.Assert(myDevice != null);
                myDevice.Reset();
                return true;
            }
            else
            {
                Console.WriteLine("GetDeviceFromResource fail");
                return false;
            }
        }
#if dont
        private void StartReader(Session session1, CubeSetup setup)
        {
            _channelAuxList = new List<ChannelAux>();

            SL508892Setup serialDev = setup.GetDeviceSetupEntry(3) as SL508892Setup;
            for (int chNum = 0; chNum < session1.GetNumberOfChannels(); chNum++)
            {

                // set channel properties
                SerialPort sPort = session1.GetChannel(chNum) as SerialPort;
                int chIndex = sPort.GetIndex();
                SerialChannelSetup serialChannel = serialDev.Channels[chIndex];
                if (null != serialChannel)
                {
                    sPort.SetMode(serialChannel.Mode);
                    sPort.SetSpeed(serialChannel.Baudrate);
                    sPort.SetParity(serialChannel.Parity);
                    sPort.SetStopBits(serialChannel.Stopbits);
                }

                // add channel to channel-list
                ChannelAux chAux = new ChannelAux(chIndex, session1);
                _channelAuxList.Add(chAux);
                chAux.Reader = new SerialReader(session1.GetDataStream(), chAux.ChannelIndex);

            }
            foreach (ChannelAux cx in _channelAuxList)
            {
                cx.AsyncResult = cx.Reader.BeginRead(200, new AsyncCallback(ReaderCallback), cx);
            }
        }
#endif
        void ReaderCallback(IAsyncResult ar)
        {
            if ((true == stopByUser)||(true==stopByWatchdog))
            {
                return;
            }
            ChannelAux chAux = ar.AsyncState as ChannelAux;
            //Console.WriteLine($"Message from channel {chAux.ChannelIndex} ");
            int chIndex = chAux.ChannelIndex;
            try
            {
                byte[] recvBytes = chAux.Reader.EndRead(ar);
                System.Diagnostics.Debug.Assert(null != recvBytes);
                System.Diagnostics.Debug.Assert(null != chAux.OriginatingSession);

                System.Diagnostics.Debug.Assert(true == chAux.OriginatingSession.IsRunning());

                //SerialPort sp = chAux.OriginatingSession.GetChannel(chIndex) as SerialPort;
                Console.WriteLine($"Message from channel {chIndex}. Length {recvBytes.Length}");
                chAux.AsyncResult = chAux.Reader.BeginRead(200, new AsyncCallback(ReaderCallback), chAux);
            }
            catch (UeiDaqException ex)
            {
                // only handle exception if the session is running
                if (chAux.OriginatingSession.IsRunning())
                {
                    if (Error.Timeout == ex.Error)
                    {
                        // Ignore timeout error, they will occur if the send button is not
                        // clicked on fast enough!
                        // Just re-initiate a new asynchronous read.
                        Console.WriteLine($"Timeout ch {chIndex}");
                        if ((false == stopByUser)&&(false==stopByWatchdog))
                        {
                            chAux.AsyncResult = chAux.Reader.BeginRead(200, new AsyncCallback(ReaderCallback), chAux);
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Reader error: {ex.Message}");
                        System.Diagnostics.Debug.Assert(false, ex.Message);
                        // tbd: close all
                        //chAux.OriginatingSession.Dispose();
                    }
                }

                System.Diagnostics.Debug.Assert(true == chAux.OriginatingSession.IsRunning());
            }
        }

        public Session BuildSerialSession(SL508892Setup deviceSetup)
        {
            Session serSession = null;
            //SL508892Setup deviceSetup = deviceSetup1 as SL508892Setup;
                //cubeSetup.GetDeviceSetupEntry(slotIndex) as SL508892Setup;
            if (null==deviceSetup)
            {
                return null;
            }
            
            string deviceuri = $"{deviceSetup.CubeUrl}Dev{deviceSetup.SlotNumber}/";

            // build serialResource string
            StringBuilder com = new StringBuilder("com");
            foreach( SerialChannelSetup ss in deviceSetup.Channels)
            {
                if (ss.IsEnabled)
                {
                    com.Append($"{ss.ChannelIndex},");
                }
            }
            string serialResource = deviceuri + com.ToString();

            try
            {
                UeiCube cube = new UeiCube(deviceSetup.CubeUrl);
                // build serial session
                if (cube.DeviceReset(deviceuri))
                {
                    serSession = new Session();
                    SerialPort port = serSession.CreateSerialPort(serialResource,
                        SerialPortMode.RS485FullDuplex,
                        SerialPortSpeed.BitsPerSecond115200,
                        SerialPortDataBits.DataBits8,
                        SerialPortParity.None,
                        SerialPortStopBits.StopBits1,
                        "");

                    // Configure timing to return serial message when either of the following conditions occurred
                    // - The termination string was detected
                    // - 100 bytes have been received
                    // - 10ms elapsed (rate set to 100Hz);
                    serSession.ConfigureTimingForMessagingIO(1000, 100.0);
                    serSession.GetTiming().SetTimeout(500);
                }
                else
                {
                    Console.WriteLine($"Failed to reset device {deviceuri}");
                }
                return serSession;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating session. {ex.Message}");
                return null;
            }
        }


        protected void CancelEventHandler(object sender, ConsoleCancelEventArgs args)
        {
            // Set cancel to true to let the Main task clean-up the I/O sessions
            args.Cancel = true;
            stopByUser = true;

        }
    }
}
