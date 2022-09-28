using System;
using log4net;

namespace UeiBridge
{
    /// <summary>
    /// Create EthernetMessage object from various sources.
    /// </summary>
    public static class EthernetMessageFactory
    {
        public static EthernetMessage CreateFromByteArray(byte[] byteMessage, out string errorString)
        {
            EthernetMessage msg = null;
            // check array validity
            // ======================
            if (byteMessage.Length < 16)
            {
                errorString = $"Byte message too short {byteMessage.Length}";
                goto exit;
            }
            if (byteMessage[0] == 0xAA && byteMessage[1] == 0x55)
            { }
            else
            {
                errorString = $"Byte message wrong preamble";
                goto exit;
            }
            UInt16 nominalLengh = BitConverter.ToUInt16(byteMessage, EthernetMessage.LengthOffset);
            if (nominalLengh != byteMessage.Length)
            {
                errorString = $"Byte message inconsistent length nominal:{nominalLengh}  actual:{byteMessage.Length}";
                goto exit;
            }
            errorString = null;

            // copy fields
            // ============
            msg = new EthernetMessage();
            msg.HeaderBytes = new byte[EthernetMessage.PayloadOffset];
            Array.Copy(byteMessage, msg.HeaderBytes, EthernetMessage.PayloadOffset);

            msg.PayloadBytes = new byte[nominalLengh - EthernetMessage.PayloadOffset];
            Array.Copy(byteMessage, EthernetMessage.PayloadOffset, msg.PayloadBytes, 0, nominalLengh - EthernetMessage.PayloadOffset);

            msg.CardType = byteMessage[EthernetMessage.CardTypeOffset];
            msg.SlotChannelNumber = byteMessage[EthernetMessage.SlotChannelNumberOffset];
            exit: return msg;
        }
        /// <summary>
        /// Create message object from device result.
        /// Might return null.
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="deviceString"></param>
        public static EthernetMessage CreateFromDevice(byte[] payload, string deviceString)
        {
            ILog _logger = log4net.LogManager.GetLogger("Root");

            int key = ProjectRegistry.Instance.GetDeviceKeyFromDeviceString(deviceString);
            if (key < 0)
            {
                _logger.Warn($"Unknown device string {deviceString}");
                return null;
            }

            EthernetMessage msg = new EthernetMessage();
            msg.CardType = (byte)key;
            msg.PayloadBytes = payload;

            System.Diagnostics.Debug.Assert(msg.CheckValid());

            return msg;
        }

        public static EthernetMessage CreateEmpty( int cardType, int payloadLength)
        {
            EthernetMessage msg = new EthernetMessage();
            msg.HeaderBytes = new byte[EthernetMessage.PayloadOffset];
            msg.PayloadBytes = new byte[payloadLength];
            msg.CardType = cardType;
            return msg;
        }

    }


}
