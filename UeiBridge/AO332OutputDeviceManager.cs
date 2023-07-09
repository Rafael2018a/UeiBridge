using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UeiBridge.Library;

namespace UeiBridge
{
    class AO332OutputDeviceManager: AO308OutputDeviceManager
    {
        public override string DeviceName => DeviceMap2.AO322Literal;
        AO332Setup _thisSetup;
        private log4net.ILog _logger = StaticMethods.GetLogger();
        //UeiDaq.Session _udeSession;
        public AO332OutputDeviceManager(DeviceSetup deviceSetup1, ISession session)
    : base(deviceSetup1, session, true)
        {
            _thisSetup = deviceSetup1 as AO332Setup;
            _ueiSession = session;
        
        }

        public AO332OutputDeviceManager(){}

        public override bool OpenDevice()
        {
            try
            {
                _attachedConverter = new AnalogConverter(AI201100Setup.PeekVoltage_upstream, AO308Setup.PeekVoltage_downstream);

                int numOfCh = _ueiSession.GetNumberOfChannels();
                System.Diagnostics.Debug.Assert(numOfCh == 32);

                Task.Factory.StartNew(() => OutputDeviceHandler_Task());

                var range = _ueiSession.GetDevice().GetAORanges();
                //AO308Setup ao308 = _deviceSetup as AO308Setup;
                //System.Diagnostics.Debug.Assert(this.IsBlockSensorActive.HasValue);
                EmitInitMessage($"Init success: {DeviceName} . { numOfCh} channels. Range {range[0].minimum},{range[0].maximum}V. Listening on {_thisSetup.LocalEndPoint.ToIpEp()}");

                _isDeviceReady = true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                return false;
            }
            return true;

        }
    }
}
