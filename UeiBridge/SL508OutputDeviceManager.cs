using System;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using UeiDaq;
using UeiBridgeTypes;

using bytearray = System.Array;
using System.Timers;

namespace UeiBridge
{
    class SL508OutputDeviceManager : OutputDevice
    {
        // publics
        public override string InstanceName { get; } //=> _instanceName;
        public override string DeviceName => "SL-508-892";
        // privates
        log4net.ILog _logger = StaticMethods.GetLogger();
        IConvert _attachedConverter;
        List<byte[]> _lastMessagesList = new List<byte[]>();
        Session _serialSession;
        int _sentBytesAcc = 0;
        int _numberOfSentMessages = 0;
        List<SerialWriter> _serialWriterList = new List<SerialWriter>();

        public SL508OutputDeviceManager(DeviceSetup setup, Session serialSession) : base(setup)
        {
            System.Diagnostics.Debug.Assert(null != serialSession);
            _serialSession = serialSession;
            InstanceName = $"{DeviceName}/Slot{ setup.SlotNumber}/Output";
            _attachedConverter = StaticMethods.CreateConverterInstance(DeviceName);

            // init message list
            for (int i = 0; i < serialSession.GetNumberOfChannels(); i++)
            {
                _lastMessagesList.Add(null);
            }
        }
        public SL508OutputDeviceManager() : base(null)// (default c-tor must be present)
        { }
        public override bool OpenDevice()
        {
            //_logger.Debug($"{this.DeviceName} OpenDevice() ..... tbd");

            for (int ch = 0; ch < _serialSession.GetNumberOfChannels(); ch++)
            {
                System.Threading.Thread.Sleep(10);
                SerialWriter sw = new SerialWriter(_serialSession.GetDataStream(), _serialSession.GetChannel(ch).GetIndex());
                _serialWriterList.Add(sw);
            }

            Task.Factory.StartNew(() => OutputDeviceHandler_Task());

            bool firstIteration = false;
            foreach (SerialPort port in _serialSession.GetChannels())
            {
                if (firstIteration)
                {
                    firstIteration = false;
                    _logger.Info($"*** {DeviceName} init:");
                }
                int channleIndex = port.GetIndex();
                _logger.Info($"Serial CH{channleIndex} init success. {port.GetMode()}  {port.GetSpeed()}.");////{c111.GetResourceName()}");
            }

            _isDeviceReady = true;
            return false;
        }

        public override void Dispose()
        {
            _logger.Warn("Dispose.... tbd");
        }
        public override string GetFormattedStatus()
        {
            StringBuilder formattedString = new StringBuilder();

            for (int ch = 0; ch < _lastMessagesList.Count; ch++) // do NOT use foreach since collection might be updated in other thread
            {
                byte[] last = _lastMessagesList[ch];
                if (null != last)
                {
                    int len = (last.Length > 20) ? 20 : last.Length;
                    string s = $"Payload ch{ch} ({last.Length}): {BitConverter.ToString(last).Substring(0, len * 3 - 1)}\n";
                    formattedString.Append(s);
                }
                else
                {
                    formattedString.Append("null\n");
                }
            }
            formattedString.Append($"Total messages {_numberOfSentMessages} bytes {_sentBytesAcc}");
            return formattedString.ToString();
        }

        protected override void HandleRequest(EthernetMessage request)
        {

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
                System.Diagnostics.Debug.Assert(sentBytes == request.PayloadBytes.Length);
                //System.Threading.Thread.Sleep(10);
                //_logger.Debug($"Sent ch{request.SerialChannel} {BitConverter.ToString(incomingMessage)}");
                _sentBytesAcc += sentBytes;
                _numberOfSentMessages++;
                _lastMessagesList[request.SerialChannelNumber] = request.PayloadBytes;
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

        protected override void resetLastScanTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (0== _lastMessagesList.Count)
            {
                return;
            }
            int ch = e.SignalTime.Second % _lastMessagesList.Count;
            _lastMessagesList[ch] = null;
            //throw new NotImplementedException();
            //e.SignalTime.Second
        }
        //else
        //{
        //    _logger.Warn($"No serial writer for channel {request.SerialChannel}");
        //}
        //}
        //System.Diagnostics.Debug.Assert(_deviceSession != null);
        //byte[] m = request.RequestObject as byte[];
        //_logger.Warn($"Should send to RS: {Encoding.ASCII.GetString(m)} ... TBD");
    }


}

