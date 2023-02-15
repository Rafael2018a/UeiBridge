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
    class BlockSensorManager : OutputDevice, ISend<SendObject>
    {
        public BlockSensorManager(DeviceSetup deviceSetup) : base(deviceSetup)
        {
        }

        public void Start()
        {

        }
        public override string DeviceName => throw new NotImplementedException();

        public override string InstanceName => throw new NotImplementedException();

        public override string[] GetFormattedStatus(TimeSpan interval)
        {
            throw new NotImplementedException();
        }

        public override bool OpenDevice()
        {
            throw new NotImplementedException();
        }

        public void Send(SendObject i)
        {
            throw new NotImplementedException();
        }

        protected override void HandleRequest(EthernetMessage request)
        {
            throw new NotImplementedException();
        }

        internal void SetAnalogOuputInterface(AO308OutputDeviceManager ao308)
        {
            throw new NotImplementedException();
        }
    }
}
