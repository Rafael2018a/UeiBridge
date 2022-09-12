using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UeiBridge 
{

    //class SL508Input : InputDevice
    //{
    //    public SL508Input(IEnqueue<ScanResult> targetConsumer, TimeSpan samplingInterval, string caseUrl) : base(targetConsumer, samplingInterval, caseUrl)
    //    {
    //    }

    //    public override string GetFormattedStatus()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public override void Start()
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

    class SL508Output : OutputDevice
    {
        log4net.ILog _logger = log4net.LogManager.GetLogger("Root");
        IConvert _attachedConverter;
        public SL508Output()
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
            byte[] m = request.RequestObject as byte[];
        
            _logger.Info($"Should send to RS: {Encoding.ASCII.GetString(m)}");
        }
    }

    /// <summary>
    /// "SL-508-892"
    /// </summary>
    class SL508DeviceManager : InputDevice
    {
        log4net.ILog _logger = log4net.LogManager.GetLogger("Root");
        //SL508Input _serialInput;
        SL508Output _serialOutput;
        public override string DeviceName => "SL-508-892";
        public SL508DeviceManager(IEnqueue<ScanResult> targetConsumer, TimeSpan samplingInterval, string caseUrl) : base(targetConsumer, samplingInterval, caseUrl)
        {
        }

        //internal SL508Input SerialInput { get => _serialInput; set => _serialInput = value; }
        internal SL508Output SerialOutput { get => _serialOutput; set => _serialOutput = value; }

        public override string GetFormattedStatus()
        {
            return "SL-508-892 input handler not ready yet";
        }

        public override void Start()
        {
            _logger.Info("Starting serial input manager...  TBD ");

        }
    }
}
