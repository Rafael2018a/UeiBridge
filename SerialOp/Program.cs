using CommandLine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using UeiBridge;
using UeiBridge.Library;
using UeiBridge.Library.CubeSetupTypes;
using UeiDaq;

/// <summary>
/// SerialOp  pdna://192.168.100.3/Dev3
/// SerialOp  pdna://192.168.100.3/Dev0  --loop 100 --length 50 --timeout 10
/// </summary>

namespace SerialOp
{
    public class Program
    {
        //private Session _serialSession;
        //List<ChannelAux> _channelAuxList;

        bool stopByUser1 = false;
        //bool stopByWatchdog = false;
        ParserResult<Options> _parseResult;

        static void Main(string[] args)
        {
            Program p = new Program();
            p.Run(args);
            Console.WriteLine("Any key to exit...");
            Console.ReadKey();
        }

        void Run(string[] args)
        {
            // register CTRL + c handler
            Console.CancelKeyPress += new ConsoleCancelEventHandler(CancelEventHandler);
            Console.WriteLine("^C to stop");

            // parse command line args
            _parseResult = CommandLine.Parser.Default.ParseArguments<Options>(args);

            var r = _parseResult.WithParsed<Options>(opts =>
            {
                LoadAndRun(opts);
            }
            );
        }
        SL508SuperManager super;
        private void LoadAndRun(Options opts)
        {
            string setupFilename;
            //string localPath;
            int deviceSlotIndex = 0; // slot index in given cube


            // get setup file name and device slot index
            if (null != opts.DeviceUri)
            {
                Uri parsedUri;
                if (Uri.TryCreate(opts.DeviceUri, UriKind.Absolute, out parsedUri))
                {

                    IPAddress ip;
                    if (IPAddress.TryParse(parsedUri.Host, out ip))
                    {
                        int last = ip.GetAddressBytes()[3];
                        setupFilename = $"Cube{last}.config";

                        string localPath = parsedUri.LocalPath;
                        if (localPath.StartsWith("/Dev"))
                        {
                            string s = localPath.Substring(4);
                            Int32.TryParse(s, out deviceSlotIndex);
                        }
                    }
                    else
                    {
                        Console.WriteLine("bad ip");
                        return;
                    }
                }
                else
                {
                    Console.WriteLine("bad uri");
                    return;
                }
            }
            else
            {
                Console.WriteLine("null uri");
                return;
            }

            // load setup file
            FileInfo setupfile = new FileInfo(setupFilename);
            SL508892Setup deviceSetup;
            if (setupfile.Exists)
            {
                CubeSetup _cubeSetup;
                var csl = new CubeSetupLoader(setupfile);
                if (null == csl.CubeSetupMain)
                {
                    Console.WriteLine($"Failed to load setup file {setupfile.FullName}");
                    return;
                }
                _cubeSetup = csl.CubeSetupMain;

                deviceSetup = _cubeSetup.GetDeviceSetupEntry(deviceSlotIndex) as SL508892Setup;
            }
            else
            {
                Console.WriteLine($"setup file {setupfile.FullName} doesn't exist");
                return;
            }

            super = new SL508SuperManager();
            super.StartDevice(deviceSetup);

            do
            {
                System.Threading.Thread.Sleep(100);
            } while (false == stopByUser1);
            Console.WriteLine("Dispose");
            super.Dispose();
        }
        protected void CancelEventHandler(object sender, ConsoleCancelEventArgs args)
        {
            args.Cancel = true;
            stopByUser1 = true;
            Console.WriteLine("stopByUser1 = true");
        }

#if dont
        SL508DeviceManager deviceManager; // tbd
        void WatchdogLoop_moved(SL508892Setup deviceSetup)
        {
           

            if (null == deviceSetup)
            {
                return;
            }
            do // watchdog loop
            {
                // set session
                // -----------
                Session serSession = SL508DeviceManager.BuildSerialSession2(deviceSetup);
                if (null == serSession)
                {
                    break;// loop
                }
                serSession.Start();

                Console.WriteLine($"Listening on {serSession.GetDevice().GetResourceName()}");

                // set device manager and watchdog
                // -------------------------------
                deviceManager = new SL508DeviceManager(null, deviceSetup, serSession);
                SerialWatchdog swd = new SerialWatchdog(new Action<string>((i) => { stopByWatchdog = true; deviceManager.Stop(); }));
                deviceManager.SetWatchdog(swd);
                if (false == deviceManager.StartDevice())
                {
                    break;// loop
                }
                deviceManager.WaitAll();
                // sleep
                // -----

                //int seed = 10;
                //do
                //{
                //    System.Threading.Thread.Sleep(1000);
                //    List<byte[]> msgs = StaticMethods.Make_SL508Down_Messages(++seed);
                //    foreach (byte[] m in msgs)
                //    {
                //        deviceManager.Enqueue(m); //new byte[] { 0, 1, 2 });
                //    }
                //} while (false == stopByUser && false == stopByWatchdog);

                // Display statistics 
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
                if (true == stopByWatchdog)
                {
                    System.Threading.Thread.Sleep(1000);
                    stopByWatchdog = false;
                }
                swd = null;
            } while (false == stopByUser);
        }
#endif
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

        int line = 0;
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
                Console.WriteLine($"({++line}) Message from channel {chIndex}. Length {recvBytes.Length}");
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
#endif

        public Session BuildSerialSession(SL508892Setup deviceSetup)
        {
            Session serSession = null;
            if (null == deviceSetup)
            {
                return null;
            }

            string deviceuri = $"{deviceSetup.CubeUrl}Dev{deviceSetup.SlotNumber}/";

            // build serialResource string
            StringBuilder com = new StringBuilder("com");
            foreach (SerialChannelSetup ss in deviceSetup.Channels)
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


    }
}
