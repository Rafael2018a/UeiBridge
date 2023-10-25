using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using UeiBridge.Library;

namespace UeiBridge.CubeSetupTypes
{
    public class AO308Setup : DeviceSetup
    {
        [XmlIgnore]
        public static double PeekVoltage_downstream => 10.0;

        public AO308Setup()
        {
        }
        public AO308Setup(EndPoint localEndPoint, UeiDeviceInfo device) : base(localEndPoint, null, device)
        {
        }
    }
    public class BlockSensorSetup : AO308Setup
    {
        //public const int BlockSensorSlotNumber = 32;
        public bool IsActive { get; set; }
        //public int AnalogCardSlot { get; set; }
        public int DigitalCardSlot { get; set; }
        public BlockSensorSetup(EndPoint localEndPoint, UeiDeviceInfo deviceInfo) : base(localEndPoint, deviceInfo)
        {
            //SlotNumber = AnalogCardSlot;
        }

        protected BlockSensorSetup()
        {
        }
    }
    public class SimuAO16Setup : AO308Setup
    {
        public SimuAO16Setup()
        {
        }
        public SimuAO16Setup(EndPoint localEndPoint, UeiDeviceInfo device) : base(localEndPoint, device)
        {
        }
    }
    public class AI201100Setup : DeviceSetup
    {
        [XmlIgnore]
        public static double PeekVoltage_upstream => 12.0;
        public AI201100Setup(EndPoint destEndPoint, UeiDeviceInfo device) : base(null, destEndPoint, device)
        {
        }
        protected AI201100Setup()
        {
        }
    }

    public class CAN503Setup : DeviceSetup
    {
        public List<CANChannelSetup> Channels;
        const int _numberOfChannels = 4;

        public CAN503Setup()
        {
        }

        public CAN503Setup(EndPoint localEndPoint, EndPoint destEndPoint, UeiDeviceInfo device) : base(localEndPoint, destEndPoint, device)
        {
            Channels = new List<CANChannelSetup>();

            for (int chIndex = 0; chIndex < _numberOfChannels; chIndex++)
            {
                Channels.Add(new CANChannelSetup(chIndex));
            }

        }

    }
    public class DIO403Setup : DeviceSetup
    {
        public List<DIOChannel> IOChannelList { get; set; }
        //public Direction[] BitOctets;
        public DIO403Setup()
        {
        }

        public DIO403Setup(EndPoint localEndPoint, EndPoint destEndPoint, UeiDeviceInfo device, int numberOfChannels) : base(localEndPoint, destEndPoint, device)
        {
            IOChannelList = new List<DIOChannel>();
            for (byte ch = 0; ch < numberOfChannels; ch++)
            {
                MessageWay w = (ch % 2 == 0) ? MessageWay.upstream : MessageWay.downstream;
                IOChannelList.Add(new DIOChannel(ch, w));
            }
        }
    }
    public class SimuDIO64Setup : DIO403Setup
    {
        public SimuDIO64Setup(EndPoint localEndPoint, EndPoint destEndPoint, UeiDeviceInfo device) : base(localEndPoint, destEndPoint, device, 4)
        {
            IOChannelList = new List<DIOChannel>();
            for (byte ch = 0; ch < 4; ch++)
            {
                MessageWay w = (ch % 2 == 0) ? MessageWay.upstream : MessageWay.downstream;
                IOChannelList.Add(new DIOChannel(ch, w));
            }

        }
    }
    public class DIO470Setup : DeviceSetup
    {
        public DIO470Setup()
        {
        }

        public DIO470Setup(EndPoint localEndPoint, UeiDeviceInfo device) : base(localEndPoint, null, device)
        {
        }
    }
    public class SL508892Setup : DeviceSetup
    {
        public List<SerialChannelSetup> Channels;
        const int _numberOfSerialChannels = 8;
        public SL508892Setup()
        {
        }

        public SL508892Setup(EndPoint localEndPoint, EndPoint destEndPoint, UeiDeviceInfo device) : base(localEndPoint, destEndPoint, device)
        {
            Channels = new List<SerialChannelSetup>();

            for (int chIndex = 0; chIndex < _numberOfSerialChannels; chIndex++)
            {
                Channels.Add(new SerialChannelSetup(chIndex, UeiDaq.SerialPortSpeed.BitsPerSecond19200));
            }
        }
    }

}
