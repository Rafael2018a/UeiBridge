﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using UeiBridge.Library.CubeSetupTypes;
using UeiDaq;

namespace SerialOp
{

    class mysite : ISite
    {
        public mysite(string n)
        {
            this.Name = n;
        }
        public IComponent Component => throw new NotImplementedException();

        public IContainer Container => throw new NotImplementedException();

        public bool DesignMode { get; set; }

        public string Name { get; set; }

        public object GetService(Type serviceType)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// serial-agent -r -ch 1
    /// </summary>
    public class Program
    {
        private Session _serialSession;
        List<ChannelAux> _channelAuxList;
        CubeSetup _cubeSetup;

        static void Main()
        {
            Program p = new Program();
            p.MainSerial();
        }


        public void MainSerial()
        {
            // register CTRL + c handler
            Console.CancelKeyPress += new ConsoleCancelEventHandler(myHandler);
            FileInfo setupfile = new FileInfo("Cube3.config");
            var csl = new CubeSetupLoader(setupfile);
            if (null == csl.CubeSetupMain)
            {
                Console.WriteLine($"File to load setup file {setupfile.FullName}");
                return;
            }
            _cubeSetup = csl.CubeSetupMain;

            var ds = _cubeSetup.GetDeviceSetupEntry(0) as SL508892Setup;
            Session session1 = BuildSerialSession( ds, 0);
            session1.Start();
            //SerialReaderTask reader = new SerialReaderTask(session1);
            //reader.Start();

            //this.StartReader(session1, _cubeSetup);

            //SL508892Setup serialDev = _cubeSetup.GetDeviceSetupEntry(3) as SL508892Setup; // slot 3
            SL508InputManager inputManager = new SL508InputManager(null, ds, session1);

            SerialWatchdog swd = new SerialWatchdog(new Action<string>((i) => { stop = true; }));
            inputManager.SetWatchdog(swd);

            inputManager.OpenDevice();

            Console.WriteLine("^C to stop");
            // sleep
            do { System.Threading.Thread.Sleep(1000); } while (false == stop);

            inputManager.Dispose();
            // dispose process
            // ===============

            _serialSession.Stop();
            System.Diagnostics.Debug.Assert(false == _serialSession.IsRunning());

            _serialSession.Dispose();

            Console.WriteLine("All Disposed");
            Console.WriteLine("Any key to exit...");
            Console.ReadKey();
        }

        bool DeviceReset(string deviceUri)
        {
            Device myDevice = DeviceEnumerator.GetDeviceFromResource(deviceUri);

            if (null != myDevice)
            {
                string devName = myDevice.GetDeviceName();
                System.Diagnostics.Debug.Assert(myDevice != null);
                myDevice.Reset();
                return true;
            }
            else
            {
                Console.WriteLine("GetDeviceFromResource fail");
                return false;
            }
        }

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

        void ReaderCallback(IAsyncResult ar)
        {
            if (true == stop)
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
                        if (false == stop)
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

        public Session BuildSerialSession(SL508892Setup deviceSetup, int slotIndex)
        {
            //SL508892Setup deviceSetup = deviceSetup1 as SL508892Setup;
                //cubeSetup.GetDeviceSetupEntry(slotIndex) as SL508892Setup;
            if (null==deviceSetup)
            {
                return null;
            }
            string deviceuri = $"{deviceSetup.CubeUrl}Dev{slotIndex}/";

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
                // build serial session
                DeviceReset(deviceuri);
                _serialSession = new Session();
                SerialPort port = _serialSession.CreateSerialPort(serialResource,
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
                _serialSession.ConfigureTimingForMessagingIO(1000, 100.0);
                _serialSession.GetTiming().SetTimeout(500);

                return _serialSession;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating session. {ex.Message}");
                return null;
            }
        }


        protected static void myHandler(object sender, ConsoleCancelEventArgs args)
        {
            // Set cancel to true to let the Main task clean-up the I/O sessions
            args.Cancel = true;
            stop = true;

        }

        static bool stop = false;


    }
}
