using System;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using UeiDaq;

namespace UeiBridge
{
    class SL508OutputDeviceManager : OutputDevice
    {
        log4net.ILog _logger = StaticMethods.GetLogger();
        IConvert _attachedConverter;
        List<byte[]> _lastMessagesList;
        //const string _termString = "\r\n";
        //SL508InputDeviceManager _serialInputManger=null;
        private string _instanceName;
        public override string DeviceName => "SL-508-892";

        //int _numberOfChannels = 1;
        public SL508OutputDeviceManager( DeviceSetup setup): base( setup)
        {
             _attachedConverter = StaticMethods.CreateConverterInstance(DeviceName);
            _lastMessagesList = new List<byte[]>();
            for (int i = 0; i < 8; i++)
            {
                _lastMessagesList.Add(null);
            }
            _instanceName = $"{DeviceName}/Slot{ setup.SlotNumber}/Output";
        }
        public SL508OutputDeviceManager(): base(null)
        { }


        //protected override IConvert AttachedConverter => _attachedConverter;

        //protected override string ChannelsString => throw new System.NotImplementedException();

        public override string InstanceName => _instanceName;

        public override void Dispose()
        {
            // do nothing. this manager relays on 508InputManger
        }
        public void Start()
        {
            Task.Factory.StartNew(() => OutputDeviceHandler_Task());
            
            //_isDeviceReady = true;
        }
        public override string GetFormattedStatus()
        {
            //return null;
            StringBuilder formattedString = new StringBuilder();
            //lock (_lastMessagesList)
            //{

            //    foreach (byte[] msg in _lastMessagesList)
            //    {
            //        int ch = 0;
            //        if (null != msg)
            //        {
            //            int len = (msg.Length > 20) ? 20 : msg.Length;
            //            string full = BitConverter.ToString(msg);
            //            string s = $"Payload ch{ch++} ({msg.Length}): {full.Substring(0, len * 3 - 1)}\n";
            //            formattedString.Append(s);
            //        }
            //    }
            //}

            for (int ch = 0; ch < _lastMessagesList.Count; ch++) // do NOT use foreach since collection might be updated in other thread
            {
                byte[] last = _lastMessagesList[ch];
                if (null != last)
                {
                    int len = (last.Length > 20) ? 20 : last.Length;
                    string s = $"Payload ch{ch} ({last.Length}): {BitConverter.ToString(last).Substring(0, len * 3 - 1)}\n";
                    formattedString.Append(s);
                    //_lastMessagesList[ch] = null;
                }
                else
                {
                    formattedString.Append("null\n");
                }
            }
            formattedString.Append($"Total messages {_numberOfSentMessages} bytes {_sentBytesAcc}");
            return formattedString.ToString();

        }

        int _sentBytesAcc = 0;
        int _numberOfSentMessages = 0;
        protected override void HandleRequest(EthernetMessage request)
        {
            throw new NotImplementedException();
        }

        protected void HandleRequest(DeviceRequest request)
        {
            
            SL508InputDeviceManager _serialInputManager = ProjectRegistry.Instance.SerialInputDeviceManager;

            if (null == _serialInputManager || _serialInputManager.IsReady==false)
            {
                _logger.Warn("Can't hanlde request since serialInputManager null or not ready");
                return;
            }
            //if (_isDeviceReady==false)
            //{
            //    return;
            //}
            byte []  incomingMessage = request.RequestObject as byte[];
            System.Diagnostics.Debug.Assert(incomingMessage != null);
            System.Diagnostics.Debug.Assert(request.SerialChannel >= 0);
            if (_serialInputManager?.SerialWriterList != null)
            {
                if (_serialInputManager.SerialWriterList.Count > request.SerialChannel)
                {
                    if (null != _serialInputManager.SerialWriterList[request.SerialChannel])
                    {
                        int sentBytes = 0;
                        try
                        {
                            sentBytes = _serialInputManager.SerialWriterList[request.SerialChannel].Write(incomingMessage);
                            System.Diagnostics.Debug.Assert(sentBytes == incomingMessage.Length);
                            //System.Threading.Thread.Sleep(10);
                            //_logger.Debug($"Sent ch{request.SerialChannel} {BitConverter.ToString(incomingMessage)}");
                            _sentBytesAcc += sentBytes;
                            _numberOfSentMessages++;
                            _lastMessagesList[request.SerialChannel] = incomingMessage;
                        }
                        catch (UeiDaqException ex)
                        {
                            _logger.Warn($"{ex.Message}. Total {_sentBytesAcc} bytes in {_numberOfSentMessages} messages.");
                        }
                        catch(Exception ex)
                        {
                            _logger.Warn($"{ex.Message}. Total {_sentBytesAcc} bytes in {_numberOfSentMessages} messages.");
                        }
                    }
                    else
                    {
                        //if (false == _serialInputManager.InDisposeState)
                        {
                            _logger.Warn("Failed to send serial message. SerialWriter==null)");
                        }
                    }
                }
                else
                {
                    _logger.Warn($"No serial writer for channel {request.SerialChannel}");
                }
            }
            //System.Diagnostics.Debug.Assert(_deviceSession != null);
            //byte[] m = request.RequestObject as byte[];
            //_logger.Warn($"Should send to RS: {Encoding.ASCII.GetString(m)} ... TBD");
        }

        public override bool OpenDevice()
        {
            _logger.Debug($"{this.DeviceName} OpenDevice() ..... tbd");
            return false;
        }

    }
}
