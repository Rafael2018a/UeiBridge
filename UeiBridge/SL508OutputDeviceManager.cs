using System;
using System.Text;
using UeiDaq;

namespace UeiBridge
{
    class SL508OutputDeviceManager : OutputDevice
    {
        log4net.ILog _logger = log4net.LogManager.GetLogger("Root");
        IConvert _attachedConverter;
        //const string _termString = "\r\n";
        
        //int _numberOfChannels = 1;
        public SL508OutputDeviceManager()
        {
            
            _attachedConverter = StaticMethods.CreateConverterInstance(DeviceName);
        }

        public override string DeviceName => "SL-508-892";

        public override IConvert AttachedConverter => _attachedConverter;

        protected override string ChannelsString => throw new System.NotImplementedException();

        public override void Dispose()
        {
            CloseDevice();
        }

        public override string GetFormattedStatus()
        {
            return "SL-508-892 output: not ready yet";
        }

        protected override void HandleRequest(DeviceRequest request)
        {

            System.Diagnostics.Debug.Assert(_deviceSession != null);
            byte[] m = request.RequestObject as byte[];
            _logger.Warn($"Should send to RS: {Encoding.ASCII.GetString(m)} ... TBD");
        }

    }
}
