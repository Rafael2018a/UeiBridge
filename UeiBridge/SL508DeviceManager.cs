using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UeiBridge
{

    /// <summary>
    /// "SL-508-892"
    /// </summary>
    class SL508DeviceManager : InputDevice
    {
        log4net.ILog _logger = log4net.LogManager.GetLogger("Root");
        //SL508Input _serialInput;
        //SL508OutputDeviceManager _serialOutput;
        public override string DeviceName => "SL-508-892";

        public override IConvert AttachedConverter => throw new NotImplementedException();

        public SL508DeviceManager(IEnqueue<ScanResult> targetConsumer, TimeSpan samplingInterval, string caseUrl) : base(targetConsumer, samplingInterval, caseUrl)
        {
        }

        //internal SL508Input SerialInput { get => _serialInput; set => _serialInput = value; }
        //internal SL508OutputDeviceManager SerialOutput { get => _serialOutput; set => _serialOutput = value; }

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
