﻿using System;
using System.Text;
using System.Collections.Generic;
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

        //int _numberOfChannels = 1;
        public SL508OutputDeviceManager()
        {
            //if (null != ProjectRegistry.Instance.SerialInputDeviceManager)
            {
                //_serialInputManger = ProjectRegistry.Instance.SerialInputDeviceManager;
                _attachedConverter = StaticMethods.CreateConverterInstance(DeviceName);
            }
            //else
            //{
            //    _logger.Warn("Can't start SL508OutputDeviceManager since SerialInputDeviceManager=null");
            //}
            _lastMessagesList = new List<byte[]>();
            for (int i = 0; i < 8; i++)
            {
                _lastMessagesList.Add(null);
            }
        }

        public override string DeviceName => "SL-508-892";

        public override IConvert AttachedConverter => _attachedConverter;

        protected override string ChannelsString => throw new System.NotImplementedException();

        public override void Dispose()
        {
            // do nothing. this manager relays on 508InputManger
        }
        public override void Start()
        {
            base.Start();
            //_isDeviceReady = true;
        }
        public override string GetFormattedStatus()
        {
            StringBuilder formattedString = new StringBuilder();
            for(int ch=0; ch<8; ch++)
            {
                byte[] last = _lastMessagesList[ch];
                if (null!=last)
                {
                    int len = (last.Length > 20) ? 20 : last.Length;
                    string s = $"Payload ch{ch}: {BitConverter.ToString(last).Substring(0, len * 3 - 1)}\n";
                    formattedString.Append(s);
                }
            }
            return formattedString.ToString();

            //if (null != _lastMessage)
            //{
            //    int l = (_lastMessage.Length > 20) ? 20 : _lastMessage.Length;
            //    System.Diagnostics.Debug.Assert(l > 0);
            //    formattedString = "First bytes: " + BitConverter.ToString(_lastMessage).Substring(0, l * 3 - 1) + "\n line2";
            //}
            //return formattedString;
        }

        int _sentBytesAcc = 0;
        int _numberOfSentMessages = 0;

        protected override void HandleRequest(DeviceRequest request)
        {
            
            SL508InputDeviceManager _serialInputManager = ProjectRegistry.Instance.SerialInputDeviceManager;
            if (null == _serialInputManager)
            {
                _logger.Warn("Can't hanlde request since serialInputManager==null");
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
                        if (false == _serialInputManager.InDisposeState)
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

    }
}
