using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using UeiDaq;

namespace SerialLoopbackAsync
{
    class Program
    {
        static string[] resourceStr = { "pdna://192.168.100.40/dev0/COM0:1"};
        static Session[] SrSession = new Session[resourceStr.Length];
        static int readSize = 100;
        static bool stop = false;

        //define structure of session and channel index to pass to Async Read & Write Callback method.
        public struct WriteType
        {
            public int sessionIndex;
            public int chanIndex;
            public SerialWriter sw;
            public AsyncCallback callback;
            public IAsyncResult result;
        }
        public struct ReadType
        {
            public int sessionIndex;
            public int chanIndex;
            public SerialReader sr;
            public AsyncCallback callback;
            public IAsyncResult result;
        }

        protected static void myHandler(object sender, ConsoleCancelEventArgs args)
        {
            // Set cancel to true to let the Main task clean-up the I/O sessions
            args.Cancel = true;
            stop = true;
        }
        static void DasReset()//reset any "run away" session
        {

            int deviceNumber;
            int powerDev = 0;
            Device myDevice;
            string devCollName;


            DeviceCollection devColl = new DeviceCollection("pdna://192.168.100.40");

            foreach (Device dev in devColl)
            {
                if (dev != null)
                {
                    devCollName = dev.GetDeviceName();

                    if (devCollName == "DNRP-40")
                    {
                        powerDev = dev.GetIndex();
                    }
                }
            }


            for (deviceNumber = 0; deviceNumber <= powerDev - 2; deviceNumber++)
            {
                string rNAME = @"pdna://192.168.100.40/Dev" + deviceNumber.ToString().Trim() + @"/";
                myDevice = new Device();
                myDevice = DeviceEnumerator.GetDeviceFromResource(rNAME);
                string devName = myDevice.GetDeviceName();
                if (myDevice != null)
                {
                    myDevice.Reset();
                }
            }


        }
        //setting up writer callback method
        static void WriterCallback(IAsyncResult ar)
        {
            string message;

            WriteType state = (WriteType)ar.AsyncState;

            Thread.Sleep(100);
            if (stop) return;
            message = string.Format("This message is coming from device {0} - Channel {1}\r\n", state.sessionIndex, state.chanIndex);
            //System.Console.WriteLine("Writer CB - indexType.sessionIndex is {0} - indexType.chanIndex is {1}", state.sessionIndex, state.chanIndex);
           try
            {
                state.sw.EndWrite(ar);

                if (SrSession[state.sessionIndex] != null && SrSession[state.sessionIndex].IsRunning())
                {
                    //System.Console.WriteLine("Writer CB - indexType.sessionIndex is {0} - indexType.chanIndex is {1}", state.sessionIndex, state.chanIndex);
                    //rearm the reader to read again
                    state.result = state.sw.BeginWrite(state.callback, state, System.Text.Encoding.ASCII.GetBytes(message));
                }
            }
            catch (UeiDaqException ex)
            {
                if (SrSession[state.sessionIndex].IsRunning())
                {
                    if (Error.Timeout == ex.Error)
                    {
                        // Just reinitiate a new asynchronous read.
                        state.result = state.sw.BeginWrite(state.callback, state, System.Text.Encoding.ASCII.GetBytes(message));
                        Console.WriteLine(" Writer Timeout");
                    }
                    else
                    {
                        SrSession[state.sessionIndex].Dispose();
                        SrSession[state.sessionIndex] = null;
                    }
                }
            }
        }
        

        static void PrintRXData(byte[] recvBytes)
        {
            System.Console.WriteLine(System.Text.Encoding.ASCII.GetString(recvBytes));
        }
        //setting up reader callback method
        static void ReaderCallback(IAsyncResult ar)
        {
            ReadType state = (ReadType)(ar.AsyncState);
            if (stop) return;
            //System.Console.WriteLine("Reader CB - indexType.sessionIndex is {0} - indexType.chanIndex is {1}", indexType.sessionIndex, indexType.chanIndex);
            try
            {
                byte[] recvBytes = state.sr.EndRead(ar);
                if (recvBytes.Length > 0)
                {
                    PrintRXData(recvBytes);//print out the rx data.
                }
                if(SrSession[state.sessionIndex] != null&& SrSession[state.sessionIndex].IsRunning())
                {
                    //rearm the reader to read again
                    state.result = state.sr.BeginRead(readSize, state.callback, state);
                }
            }
            catch (UeiDaqException ex)
            {
                if (SrSession[state.sessionIndex].IsRunning())
                {
                    if (Error.Timeout == ex.Error)
                    {
                        
                        // Just reinitiate a new asynchronous read.
                        state.result = state.sr.BeginRead(readSize, state.callback, state);
                        //Console.WriteLine("Reader Timeout");
                    }
                    else
                    {
                        SrSession[state.sessionIndex].Dispose();
                        SrSession[state.sessionIndex] = null;

                    }
                }
            }
        }
        static void Main(string[] args)
        {
            Console.CancelKeyPress += new ConsoleCancelEventHandler(myHandler);
            int srIndex,srChanIndex;
            string message;
            DasReset();
            WriteType[][] writeType = new WriteType[resourceStr.Length][];
            ReadType[][] readType = new ReadType[resourceStr.Length][];
            try
            {
                for (srIndex = 0; srIndex < resourceStr.Length; srIndex++) //create 1 session per layer
                {
                    SrSession[srIndex] = new Session();
                    SrSession[srIndex].CreateSerialPort(resourceStr[srIndex], SerialPortMode.RS485FullDuplex,
                                                        SerialPortSpeed.BitsPerSecond38400,
                                                        SerialPortDataBits.DataBits8,
                                                        SerialPortParity.None,
                                                        SerialPortStopBits.StopBits1,
                                                        "\r\n");
                    SrSession[srIndex].ConfigureTimingForMessagingIO(1000, 100.0);
                    SrSession[srIndex].GetTiming().SetTimeout(5000);

                    writeType[srIndex] = new WriteType[SrSession[srIndex].GetNumberOfChannels()];
                    readType[srIndex] = new ReadType[SrSession[srIndex].GetNumberOfChannels()];

                    //create 1 serial reader and 1 serial writer per channel
                    for (srChanIndex = 0; srChanIndex < SrSession[srIndex].GetNumberOfChannels(); srChanIndex++)
                    {
                        int channel = SrSession[srIndex].GetChannel(srChanIndex).GetIndex();
                        writeType[srIndex][srChanIndex] = new WriteType();
                        writeType[srIndex][srChanIndex].sessionIndex = srIndex;
                        writeType[srIndex][srChanIndex].chanIndex = channel;
                        writeType[srIndex][srChanIndex].sw = new SerialWriter(SrSession[srIndex].GetDataStream(), channel);
                        readType[srIndex][srChanIndex] = new ReadType();
                        readType[srIndex][srChanIndex].sessionIndex = srIndex;
                        readType[srIndex][srChanIndex].chanIndex = channel;
                        readType[srIndex][srChanIndex].sr = new SerialReader(SrSession[srIndex].GetDataStream(), channel);
                    }
                    
                    SrSession[srIndex].Start();


                    for (srChanIndex = 0; srChanIndex < SrSession[srIndex].GetNumberOfChannels(); srChanIndex++)
                    {
                        message = string.Format("This message is coming from device {0} - Channel {1}\r\n", srIndex, srChanIndex);

                        writeType[srIndex][srChanIndex].callback = new AsyncCallback(WriterCallback);
                        System.Console.WriteLine("portType.sessionIndex is {0} - portType.chanIndex is {1}",writeType[srIndex][srChanIndex].sessionIndex,writeType[srIndex][srChanIndex].chanIndex);
                        //writeType[srIndex][srChanIndex].result = writeType[srIndex][srChanIndex].sw.BeginWrite(writeType[srIndex][srChanIndex].callback, writeType[srIndex][srChanIndex], System.Text.Encoding.ASCII.GetBytes(message));
                        try {
                            writeType[srIndex][srChanIndex].sw.Write(System.Text.Encoding.ASCII.GetBytes(string.Format("This message is coming from device {0} - Channel {1}\r\n", srIndex, srChanIndex)));
                        } catch (UeiDaqException ex) {
                            //System.Console.WriteLine("Write Error: {0} - on device number: {1} {2}", ex, srIndex, srChanIndex);
                        }

                        readType[srIndex][srChanIndex].callback = new AsyncCallback(ReaderCallback);
                        //readType[srIndex][srChanIndex].result = readType[srIndex][srChanIndex].sr.BeginRead(readSize, readType[srIndex][srChanIndex].callback, readType[srChanIndex]);
                        try {
                            byte[] recv = readType[srIndex][srChanIndex].sr.Read(readSize);
                            if (recv.Length > 0) {
                                PrintRXData(recv);
                            }
                        } catch (UeiDaqException ex) {
                            //System.Console.WriteLine("Read Error: {0} - on device number: {1} {2}", ex, srIndex, srChanIndex);
                        }
                    }
                }
                while (!stop)
                {
                    for (srIndex = 0; srIndex < resourceStr.Length; srIndex++)
                    {
                        for (srChanIndex = 0; srChanIndex < SrSession[srIndex].GetNumberOfChannels(); srChanIndex++)
                        {
                            try {
                                writeType[srIndex][srChanIndex].sw.Write(System.Text.Encoding.ASCII.GetBytes(string.Format("This message is coming from device {0} - Channel {1}\r\n", srIndex, srChanIndex)));
                            } catch (UeiDaqException ex) {
                                System.Console.WriteLine("Write Error: {0} - on device number: {1} {2}", ex, srIndex, srChanIndex);
                            }

                            try {
                                byte[] recv = readType[srIndex][srChanIndex].sr.Read(readSize);
                                if (recv.Length > 0) {
                                    PrintRXData(recv);
                                }
                            } catch (UeiDaqException ex) {
                                System.Console.WriteLine("Read Error: {0} - on device number: {1} {2}", ex, srIndex, srChanIndex);
                            }
                            Thread.Sleep(100);
                        }
                    }
                    Thread.Sleep(1000);
                }
                for (srIndex = 0; srIndex < resourceStr.Length; srIndex++)
                {                    
                    SrSession[srIndex].Stop();
                    SrSession[srIndex].Dispose();
                }

            }
            catch (UeiDaqException ex)
            {
                for (srIndex = 0; srIndex < resourceStr.Length; srIndex++)
                {
                    System.Console.WriteLine("Error: {0} - on device number: {1}", ex, srIndex);
                    SrSession[srIndex].Stop();
                    SrSession[srIndex].Dispose();
                }
            }
        }
    }
}
