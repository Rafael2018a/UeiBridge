using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UeiBridge.Types;

namespace UeiBridge
{
    /// <summary>
    /// Receive 
    /// </summary>
    class BlockSensorManager : OutputDevice//, ISend<SendObject>
    {
        #region === publics ====
        public override string DeviceName => "BlockSensor";
        public override string InstanceName => "BlockSensorManager";
        #endregion
        #region === privates ===
        AO308OutputDeviceManager _ao308Device;
        log4net.ILog _logger = StaticMethods.GetLogger();
        #endregion

        public BlockSensorManager(DeviceSetup deviceSetup) : base(deviceSetup)
        {
            _deviceSetup = deviceSetup;
        }
        public BlockSensorManager() : base(null) // must be here for for Activator.CreateInstance
        {
        }

        public void Start()
        {

        }
        public override string[] GetFormattedStatus(TimeSpan interval)
        {
            return new string[] { "block sensor not ready yet" };
        }

        public override bool OpenDevice()
        {
            _logger.Info($"Init success: {InstanceName} . Listening on {_deviceSetup.LocalEndPoint.ToIpEp()}");
            // device shall be opened upon first setup message (from simulator)
            return true;
        }

        protected override void HandleRequest(EthernetMessage request)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Two types of messages might reach here
        /// 1. From "DIO-403". (Actually, this is an outgoing message and a copy of it is delivered to here)
        /// 2. From ethernet. the id: _cardIdMap.Add(32, "BlockSensor"). The first message 'opens' this manager
        /// </summary>
        public override void Enqueue(byte[] byteMessage)
        {
            int digital = StaticMethods.GetCardIdFromCardName("DIO-403");
            System.Diagnostics.Debug.Assert(digital >= 0);
            
            // if message comtes from digital card
            if (byteMessage[EthernetMessage._cardTypeOffset] == digital)
            {
                // convert
                EthernetMessage.BuildEthernetMessage(byteMessage);

                // emit to analog card
                var b = StaticMethods.Make_A308Down_message();
                _ao308Device.Enqueue(b);
            }



        }
        internal void SetAnalogOuputInterface(AO308OutputDeviceManager ao308)
        {
            _ao308Device = ao308;
        }
    }
}
