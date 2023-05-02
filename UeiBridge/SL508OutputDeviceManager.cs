using System;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using UeiDaq;
using UeiBridge.Types;
using UeiBridge.Library;

using bytearray = System.Array;
using System.Timers;

namespace UeiBridge
{
    class SL508OutputDeviceManager : OutputDevice
    {
        // publics
        public override string DeviceName => "SL-508-892";
        // privates
        log4net.ILog _logger = StaticMethods.GetLogger();
        IConvert _attachedConverter;
        List<ViewerItem<byte[]>> _lastScanList = new List<ViewerItem<byte[]>>();
        SL508Session _serialSession;
        int _sentBytesAcc = 0;
        int _numberOfSentMessages = 0;
        List<SerialWriter> _serialWriterList = new List<SerialWriter>();
        Dictionary<SerialPortSpeed, int> _serialSpeedDic = new Dictionary<SerialPortSpeed, int>();
        bool _inDisposeState = false;

        public SL508OutputDeviceManager(DeviceSetup setup, SL508Session serialSession) : base(setup)
        {
            System.Diagnostics.Debug.Assert(null != serialSession);
            _serialSession = serialSession;
            _attachedConverter = StaticMethods.CreateConverterInstance(setup);
            System.Diagnostics.Debug.Assert(null != serialSession);

            // init message list
            if (null != serialSession.SerialSession)
            {
                for (int i = 0; i < serialSession.GetNumberOfChannels(); i++)
                {
                    _lastScanList.Add(null);
                }
            }
        }
        public SL508OutputDeviceManager() : base(null)// (default c-tor must be present)
        { }
        public override bool OpenDevice()
        {
            if (null == _serialSession.SerialSession)
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

            Task.Factory.StartNew(() => OutputDeviceHandler_Task());

            bool firstIteration = true;
            foreach (SerialPort port in _serialSession.GetChannels())
            {
                if (firstIteration)
                {
                    firstIteration = false;
                    _logger.Info($"Init success {InstanceName}. Listening on {_deviceSetup.LocalEndPoint.ToIpEp()}");
                }
                int channleIndex = port.GetIndex();
                //_logger.Info($"CH{channleIndex} {port.GetMode()} {port.GetSpeed()}.");////{c111.GetResourceName()}");

            }

            _serialSpeedDic.Add(SerialPortSpeed.BitsPerSecond110, 110);
            _serialSpeedDic.Add(SerialPortSpeed.BitsPerSecond300, 300);
            _serialSpeedDic.Add(SerialPortSpeed.BitsPerSecond600, 600);
            _serialSpeedDic.Add(SerialPortSpeed.BitsPerSecond1200, 1200);
            _serialSpeedDic.Add(SerialPortSpeed.BitsPerSecond2400, 2400);
            _serialSpeedDic.Add(SerialPortSpeed.BitsPerSecond4800, 4800);
            _serialSpeedDic.Add(SerialPortSpeed.BitsPerSecond9600, 9600);
            _serialSpeedDic.Add(SerialPortSpeed.BitsPerSecond14400, 14400);
            _serialSpeedDic.Add(SerialPortSpeed.BitsPerSecond19200, 19200);
            _serialSpeedDic.Add(SerialPortSpeed.BitsPerSecond28800, 28800);
            _serialSpeedDic.Add(SerialPortSpeed.BitsPerSecond38400, 38400);
            _serialSpeedDic.Add(SerialPortSpeed.BitsPerSecond57600, 57600);
            _serialSpeedDic.Add(SerialPortSpeed.BitsPerSecond115200, 115200);
            _serialSpeedDic.Add(SerialPortSpeed.BitsPerSecond128000, 128000);
            _serialSpeedDic.Add(SerialPortSpeed.BitsPerSecond250000, 250000);
            _serialSpeedDic.Add(SerialPortSpeed.BitsPerSecond256000, 256000);
            _serialSpeedDic.Add(SerialPortSpeed.BitsPerSecond1000000, 1000000);

            _isDeviceReady = true;
            return true;
        }

        public override void Dispose()
        {
            _inDisposeState = true;
            base.CloseCurrentSession();
            //return;
            for (int ch = 0; ch < _serialWriterList.Count; ch++)
            {
                _serialWriterList[ch].Dispose();
            }
            //if (_serialSession.IsRunning())
            //{
            //    _serialSession?.Stop();
            //}
            //_serialSession.Dispose();
            //_logger.Debug("_serialSession?.Dispose();");
        }
        public override string [] GetFormattedStatus(TimeSpan interval)
        {
            //StringBuilder formattedString = new StringBuilder();
            List<string> resultList = new List<string>();

            for (int ch = 0; ch < _lastScanList.Count; ch++) // do NOT use foreach since collection might be updated in other thread
            {
                if (null == _lastScanList[ch])
                    continue;

                _lastScanList[ch].timeToLive -= interval;

                byte[] last = _lastScanList[ch].readValue;
                if ((null != last) && (_lastScanList[ch].timeToLive.Ticks > 0))
                {
                    int len = (last.Length > 20) ? 20 : last.Length;
                    string s = $"Ch{ch}: Payload=({last.Length}): {BitConverter.ToString(last).Substring(0, len * 3 - 1)}";
                    resultList.Add(s);
                }
                else
                {
                    resultList.Add($"Ch{ch}: null");
                }
            }
            resultList.Add($"Total: {_numberOfSentMessages} messages, {_sentBytesAcc} bytes ");

            return resultList.ToArray();
        }

        protected override void HandleRequest(EthernetMessage request)
        {
            if (false == _isDeviceReady)
            {
                return;
            }
            if(true == _inDisposeState)
            {
                return;
            }
            //byte []  incomingMessage = request.RequestObject as byte[];
            //System.Diagnostics.Debug.Assert(incomingMessage != null);
            //System.Diagnostics.Debug.Assert(request.SerialChannel >= 0);
            //if (_serialInputManager?.SerialWriterList != null)
            //{
            //if (_serialInputManager.SerialWriterList.Count > request.SerialChannel)

            System.Diagnostics.Debug.Assert(request.SerialChannelNumber < _serialWriterList.Count);
            UeiDaq.SerialWriter sw = _serialWriterList[request.SerialChannelNumber];
            System.Diagnostics.Debug.Assert(sw != null);

            int sentBytes = 0;
            try
            {
                sentBytes = sw.Write(request.PayloadBytes);
                // wait
                {
                    SerialPort sPort = (SerialPort)_serialSession.GetChannel(request.SerialChannelNumber);
                    SerialPortSpeed x = sPort.GetSpeed();
                    int bpsPossibe = (int)(_serialSpeedDic[x] * 0.8);
                    int bpsActual = request.PayloadBytes.Length * 8;
                    int milisec = bpsActual * 1000 / bpsPossibe;
                    System.Threading.Thread.Sleep(milisec);
                }
                //_logger.Debug($" *** Write to serial port. ch {request.SerialChannelNumber}");
                System.Diagnostics.Debug.Assert(sentBytes == request.PayloadBytes.Length);
                //System.Threading.Thread.Sleep(10);
                //_logger.Debug($"Sent ch{request.SerialChannel} {BitConverter.ToString(incomingMessage)}");
                _sentBytesAcc += sentBytes;
                _numberOfSentMessages++;
                _lastScanList[request.SerialChannelNumber] = new ViewerItem<byte[]>(request.PayloadBytes, 5000);
            }
            catch (UeiDaqException ex)
            {
                _logger.Warn($"{ex.Message}. Total {_sentBytesAcc} bytes in {_numberOfSentMessages} messages.");
            }
            catch (Exception ex)
            {
                _logger.Warn($"{ex.Message}. Total {_sentBytesAcc} bytes in {_numberOfSentMessages} messages.");
            }
            //else
            //{
            //    //if (false == _serialInputManager.InDisposeState)
            //    {
            //        _logger.Warn("Failed to send serial message. SerialWriter==null)");
            //    }
            //}
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

