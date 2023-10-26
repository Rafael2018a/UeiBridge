using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using UeiBridge.Library;
using UeiDaq;

namespace UeiBridge.CubeSetupTypes
{
    public class SerialChannelSetup
    {
        [XmlAttribute("ChannelIndex")]
        public int ChannelIndex = -1;
        public UeiDaq.SerialPortMode mode = UeiDaq.SerialPortMode.RS232;
        //[XmlElement("Baud")]
        public UeiDaq.SerialPortSpeed Baudrate { get; set; }
        public UeiDaq.SerialPortParity parity = UeiDaq.SerialPortParity.None;
        public UeiDaq.SerialPortStopBits stopbits = UeiDaq.SerialPortStopBits.StopBits1;
        public int LocalUdpPort { get; set; }

        public SerialChannelSetup(int channelIndex, UeiDaq.SerialPortSpeed speed)
        {
            this.ChannelIndex = channelIndex;
            this.Baudrate = speed;
        }
        public SerialChannelSetup()
        {
        }
    }
    public class CANChannelSetup
    {
        public CANChannelSetup()
        {
        }

        public CANChannelSetup(int channelIndex)
        {
            this.ChannelIndex = channelIndex;
        }

        public int ChannelIndex { set; get; }
        public CANPortSpeed Speed { get; set; } = CANPortSpeed.BitsPerSecond100K;
        public CANFrameFormat FrameFormat { get; set; } = CANFrameFormat.Extended;
        public CANPortMode PortMode { get; set; } = CANPortMode.Normal;
    }
    public class AppSetup
    {
        public string SelectedNicForMulticast { get; private set; } = "221.109.251.103";
        public EndPoint StatusViewerEP = new EndPoint("239.10.10.17", 5093);

        public AppSetup()
        {
            SelectedNicForMulticast = System.Configuration.ConfigurationManager.AppSettings["SelectedNicForMulticast"];
        }
    }

    public class DIOChannel
    {
        [XmlAttribute()]
        public byte OctetIndex { get; set; }
        public MessageWay Way { get; set; }
        public DIOChannel(byte octetNumber, MessageWay way)
        {
            OctetIndex = octetNumber;
            Way = way;
        }
        public DIOChannel() { }
    }
}
