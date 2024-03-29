﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UeiBridge.Library;
using UeiBridge.Library.CubeSetupTypes;
using UeiBridge.Library.Types;
using UeiDaq;

namespace UeiBridge
{

    class SL508OutputDeviceManager : OutputDevice
    {
        // publics
        public override string DeviceName => DeviceMap2.SL508Literal;
        // privates
        log4net.ILog _logger = StaticLocalMethods.GetLogger();
        //IConvert _attachedConverter;
        List<ViewItem<byte[]>> _ViewItemList = new List<ViewItem<byte[]>>(); 
        SessionAdapter _serialSession;
        int _sentBytesAcc = 0;
        int _numberOfSentMessages = 0;
        List<SerialWriter> _serialWriterList = new List<SerialWriter>();
        //Dictionary<SerialPortSpeed, int> _serialSpeedDic = new Dictionary<SerialPortSpeed, int>();
        
        private DeviceSetup _deviceSetup;
        public SL508OutputDeviceManager(DeviceSetup setup, Session serialSession) : base(setup)
        {

            if ((setup==null)||(serialSession == null))
            {
                return;
            }
            System.Diagnostics.Debug.Assert(null != serialSession);
            _serialSession = new SessionAdapter( serialSession);
            //_attachedConverter = StaticMethods.CreateConverterInstance(setup);
            System.Diagnostics.Debug.Assert(null != serialSession);
            this._deviceSetup = setup;
            // init message list
            for (int i = 0; i < serialSession.GetNumberOfChannels(); i++)
            {
                _ViewItemList.Add(null);
            }
        }
        public SL508OutputDeviceManager() { } // (default c-tor must be present)

        public override bool OpenDevice()
        {
            if ((_serialSession == null) || (_deviceSetup == null))
            {
                _logger.Warn($"Failed to open device {this.InstanceName}");
                return false;
            }
            for (int ch = 0; ch < _serialSession.GetNumberOfChannels(); ch++)
            {
                System.Threading.Thread.Sleep(10);
                SerialWriter sw = new SerialWriter(_serialSession.GetDataStream(), _serialSession.GetChannel(ch).GetIndex());
                _serialWriterList.Add(sw);
            }

            Task.Factory.StartNew(() => Task_OutputDeviceHandler());

            bool firstIteration = true;
            foreach (UeiDaq.Channel port in _serialSession.GetChannels())
            {
                if (firstIteration)
                {
                    firstIteration = false;
                    EmitInitMessage($"Init success {DeviceName}. Listening on {_deviceSetup.LocalEndPoint.ToIpEp()}");
                    //_logger.Info($"Init success {InstanceName}. Listening on {_deviceSetup.LocalEndPoint.ToIpEp()}");
                }
                int channleIndex = port.GetIndex();
                //_logger.Info($"CH{channleIndex} {port.GetMode()} {port.GetSpeed()}.");////{c111.GetResourceName()}");

            }

            //_serialSpeedDic.Add(SerialPortSpeed.BitsPerSecond110, 110);
            //_serialSpeedDic.Add(SerialPortSpeed.BitsPerSecond300, 300);
            //_serialSpeedDic.Add(SerialPortSpeed.BitsPerSecond600, 600);
            //_serialSpeedDic.Add(SerialPortSpeed.BitsPerSecond1200, 1200);
            //_serialSpeedDic.Add(SerialPortSpeed.BitsPerSecond2400, 2400);
            //_serialSpeedDic.Add(SerialPortSpeed.BitsPerSecond4800, 4800);
            //_serialSpeedDic.Add(SerialPortSpeed.BitsPerSecond9600, 9600);
            //_serialSpeedDic.Add(SerialPortSpeed.BitsPerSecond14400, 14400);
            //_serialSpeedDic.Add(SerialPortSpeed.BitsPerSecond19200, 19200);
            //_serialSpeedDic.Add(SerialPortSpeed.BitsPerSecond28800, 28800);
            //_serialSpeedDic.Add(SerialPortSpeed.BitsPerSecond38400, 38400);
            //_serialSpeedDic.Add(SerialPortSpeed.BitsPerSecond57600, 57600);
            //_serialSpeedDic.Add(SerialPortSpeed.BitsPerSecond115200, 115200);
            //_serialSpeedDic.Add(SerialPortSpeed.BitsPerSecond128000, 128000);
            //_serialSpeedDic.Add(SerialPortSpeed.BitsPerSecond250000, 250000);
            //_serialSpeedDic.Add(SerialPortSpeed.BitsPerSecond256000, 256000);
            //_serialSpeedDic.Add(SerialPortSpeed.BitsPerSecond1000000, 1000000);

            _isDeviceReady = true;
            return true;
        }

        public override void Dispose()
        {
            _inDisposeFlag = true;
            for (int ch = 0; ch < _serialWriterList.Count; ch++)
            {
                _serialWriterList[ch].Dispose();
            }
            base.TerminateMessageLoop();
            _logger.Debug($"{this.DeviceName}/Input, slot {_deviceSetup.SlotNumber}, Disposed");
        }
        public override string[] GetFormattedStatus(TimeSpan interval)
        {
            //StringBuilder formattedString = new StringBuilder();
            List<string> resultList = new List<string>();

            for (int ch = 0; ch < _ViewItemList.Count; ch++) // do NOT use foreach since collection might be updated in other thread
            {
                if (null == _ViewItemList[ch])
                    continue;

                _ViewItemList[ch].DecreaseTimeToLive( interval);

                byte[] last = _ViewItemList[ch].ReadValue;
                System.Diagnostics.Debug.Assert(last != null);
                if ((_ViewItemList[ch].TimeToLive > TimeSpan.Zero))
                {
                    int len = (last.Length > 20) ? 20 : last.Length;
                    string s = $"Ch{ch}: Payload=({last.Length}): {BitConverter.ToString(last).Substring(0, len * 3 - 1)}";
                    resultList.Add(s);
                }
                else
                {
                    _ViewItemList[ch] = null;
                    //resultList.Add($"Ch{ch}: null");
                }
            }
            if (resultList.Count > 0)
            {
                //resultList.Add($"Total: {_numberOfSentMessages} messages, {_sentBytesAcc} bytes ");
                return resultList.ToArray();
            }
            else
            {
                return null;
            }

            
        }

        protected override void HandleRequest(EthernetMessage request)
        {
            if (false == _isDeviceReady)
            {
                return;
            }
            if (true == _inDisposeFlag)
            {
                return;
            }

            System.Diagnostics.Debug.Assert(request.SerialChannelNumber < _serialWriterList.Count);
            UeiDaq.SerialWriter sw = _serialWriterList[request.SerialChannelNumber];
            System.Diagnostics.Debug.Assert(sw != null);

            int sentBytes = 0;
            try
            {
                sentBytes = sw.Write(request.PayloadBytes);

                // wait
                {
                    SerialPort sPort = _serialSession.GetChannel(request.SerialChannelNumber) as SerialPort;
                    int sp = StaticMethods.GetSerialSpeedAsInt(sPort.GetSpeed());
                    double bpsPossibe = Convert.ToDouble(sp) * 0.8;
                    int bitsToSend = request.PayloadBytes.Length * 8;
                    double waitstateMs = Convert.ToDouble( bitsToSend) /  bpsPossibe * 1000.0; // in milisec
                    System.Threading.Thread.Sleep(  Convert.ToInt32(waitstateMs));
                }
                //_logger.Debug($" *** Write to serial port. ch {request.SerialChannelNumber}");
                System.Diagnostics.Debug.Assert(sentBytes == request.PayloadBytes.Length);
                //_logger.Debug($"Sent ch{request.SerialChannel} {BitConverter.ToString(incomingMessage)}");
                _sentBytesAcc += sentBytes;
                _numberOfSentMessages++;
                _ViewItemList[request.SerialChannelNumber] = new ViewItem<byte[]>(request.PayloadBytes, TimeSpan.FromSeconds(5));
            }
            catch (UeiDaqException ex)
            {
                _logger.Warn($"{ex.Message}. Total {_sentBytesAcc} bytes in {_numberOfSentMessages} messages.");
            }
            catch (Exception ex)
            {
                _logger.Warn($"{ex.Message}. Total {_sentBytesAcc} bytes in {_numberOfSentMessages} messages.");
            }
        }

        //protected override void resetLastScanTimer_Elapsed(object sender, ElapsedEventArgs e)
        //{
        //    if (0== _lastScanList.Count)
        //    {
        //        return;
        //    }
        //    int ch = e.SignalTime.Second % _lastScanList.Count;
        //    _lastScanList[ch] = null;
        //    //throw new NotImplementedException();
        //    //e.SignalTime.Second
        //}
        //else
        //{
        //    _logger.Warn($"No serial writer for channel {request.SerialChannel}");
        //}
        //}
        //System.Diagnostics.Debug.Assert(_deviceSession != null);
        //byte[] m = request.RequestObject as byte[];
        //_logger.Warn($"Should send to RS: {Encoding.ASCII.GetString(m)} ... ");
    }


}

