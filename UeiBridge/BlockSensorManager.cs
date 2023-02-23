using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UeiBridge.Types;
using UeiBridge.Library;

namespace UeiBridge
{
    /// <summary>
    /// This manager handles the 'block sensor'.
    /// It gets a udp message which define series of voltage values.
    /// According to input from digital card, it decide into which analog output is should emit this values.
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
        public BlockSensorManager() : base(null) // must be here for Activator.CreateInstance
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
        /// 2. From Ethernet. the id: _cardIdMap.Add(32, "BlockSensor"). The first message 'opens' this manager
        /// </summary>
        public override void Enqueue(byte[] byteMessage)
        {
            // if message comes from digital card
            if (byteMessage[EthernetMessage._cardTypeOffset] == StaticMethods.GetCardIdFromCardName("DIO-403"))
            {
                // convert
                //EthernetMessage.BuildEthernetMessage(byteMessage);

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
