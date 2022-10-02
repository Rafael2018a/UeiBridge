using System;
using System.Text;
using UeiDaq;

namespace UeiBridge
{
    class SL508OutputDeviceManager : OutputDevice
    {
        log4net.ILog _logger = StaticMethods.GetLogger();
        IConvert _attachedConverter;
        //const string _termString = "\r\n";
        SL508InputDeviceManager _serialInputManger=null;

        //int _numberOfChannels = 1;
        public SL508OutputDeviceManager()
        {
            
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
            if (null != ProjectRegistry.Instance.SerialInputDeviceManager)
            {
                _serialInputManger = ProjectRegistry.Instance.SerialInputDeviceManager;
                _attachedConverter = StaticMethods.CreateConverterInstance(DeviceName);
            }
            else
            {
                _logger.Warn("Can't start SL508OutputDeviceManager since SerialInputDeviceManager=null");
            }

            base.Start();
        }
        public override string GetFormattedStatus()
        {
            return "SL-508-892 output: not ready yet"; // tbd
        }

        protected override void HandleRequest(DeviceRequest request)
        {
            byte[] m = request.RequestObject as byte[];
            System.Diagnostics.Debug.Assert(m != null);
            System.Diagnostics.Debug.Assert(request.SerialChannel >= 0);
            if (_serialInputManger?.SerialWriterList != null)
            {
                if (_serialInputManger.SerialWriterList.Count > request.SerialChannel)
                {
                    if (null != _serialInputManger.SerialWriterList[request.SerialChannel])
                    {
                        _serialInputManger.SerialWriterList[request.SerialChannel].Write(m);
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
