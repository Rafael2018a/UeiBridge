using System;
using System.Text;
using UeiDaq;

namespace UeiBridge
{
    class SL508OutputDeviceManager : OutputDevice
    {
        log4net.ILog _logger = StaticMethods.GetLogger();
        IConvert _attachedConverter;
        byte[] _lastMessage;
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
            string formattedString="";
            if (null != _lastMessage)
            {
                int l = (_lastMessage.Length > 20) ? 20 : _lastMessage.Length;
                System.Diagnostics.Debug.Assert(l > 0);
                formattedString = "First bytes: "+BitConverter.ToString(_lastMessage).Substring(0, l*3-1);
            }
            return formattedString;
        }

        protected override void HandleRequest(DeviceRequest request)
        {
            SL508InputDeviceManager _serialInputManger = ProjectRegistry.Instance.SerialInputDeviceManager;
            if (null==_serialInputManger)
            {
                _logger.Warn("Can't hanlde request since serialInputManager==null");
                return;
            }
            //if (_isDeviceReady==false)
            //{
            //    return;
            //}

            _lastMessage = request.RequestObject as byte[];
            System.Diagnostics.Debug.Assert(_lastMessage != null);
            System.Diagnostics.Debug.Assert(request.SerialChannel >= 0);
            if (_serialInputManger?.SerialWriterList != null)
            {
                if (_serialInputManger.SerialWriterList.Count > request.SerialChannel)
                {
                    if (null != _serialInputManger.SerialWriterList[request.SerialChannel])
                    {
                        try
                        {
                            _serialInputManger.SerialWriterList[request.SerialChannel].Write(_lastMessage);
                        }
                        catch(Exception ex)
                        {
                            _logger.Warn(ex.Message + ". " + ex.GetType().ToString());
                        }
                    }
                    else
                    {
                        if (false == _serialInputManger.InDisposeState)
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
