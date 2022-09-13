using System.Text;

namespace UeiBridge
{
    class SL508OutputDeviceManager : OutputDevice
    {
        log4net.ILog _logger = log4net.LogManager.GetLogger("Root");
        IConvert _attachedConverter;
        public SL508OutputDeviceManager()
        {
            _attachedConverter = StaticMethods.CreateConverterInstance(DeviceName);
        }

        public override string DeviceName => "SL-508-892";

        public override IConvert AttachedConverter => _attachedConverter;

        public override string GetFormattedStatus()
        {
            return "SL-508-892 output: not ready yet";
        }

        protected override void HandleRequest(DeviceRequest request)
        {

            // init session, if needed.
            // =======================
            if ((null == _deviceSession) || (_caseUrl != request.CaseUrl))
            {
                CloseDevice(); // if needed
                //OpenDevice(dr, DeviceName);
            }


            byte[] m = request.RequestObject as byte[];
        
            _logger.Info($"Should send to RS: {Encoding.ASCII.GetString(m)}");
        }
    }
}
