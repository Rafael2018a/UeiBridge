using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UeiBridge
{
    /// <summary>
    /// This class represents an icd dedicated datagram 
    /// that might be received or sent by this application
    /// </summary>
    public class EthernetMessage
    {
        //const int _payloadOffset = 16;
        //const int _cardTypeOffset = 5; // 1 byte
        //const int _lengthOffset = 12; // 2 bytes

        public int UnitId { get; set; }
        public int CardType { get; set; }
        public int SlotNumber { get; set; }
        public int SlotChannelNumber { get; set; }
        public byte[] PayloadBytes { get; set; }
        public byte[] HeaderBytes { get; set; }
        public int _debugSerial { get; set; } // serial number of message

        public static int PayloadOffset => 16;// _payloadOffset;

        public static int CardTypeOffset => 5;// _cardTypeOffset;

        public static int SlotChannelNumberOffset => 7;

        public static int LengthOffset => 12;// _lengthOffset;

        /// <summary>
        /// Convert to current instance byte array (for sending through ethernet)
        /// Might return null.
        /// </summary>
        byte[] ToByteArray()
        {
            if (!CheckValid())
                return null;

            byte[] messageBytes = new byte[PayloadOffset + PayloadBytes.Length];
            Array.Clear(messageBytes, 0, messageBytes.Length);

            // card type
            messageBytes[CardTypeOffset] = (byte)CardType;
            messageBytes[SlotChannelNumberOffset] = (byte)SlotChannelNumber;

            // message length
            byte[] twobytes = BitConverter.GetBytes(messageBytes.Length);
            Array.Copy(twobytes, 0, messageBytes, LengthOffset, twobytes.Length);
            
            // payload
            Array.Copy(PayloadBytes, 0, messageBytes, PayloadOffset, PayloadBytes.Length);

            return messageBytes;
        }
        /// <summary>
        /// Convert to current instance byte array (for sending through ethernet)
        /// Might return null.
        /// </summary>
        public byte[] ToByteArrayUp()
        {
            var result = ToByteArray();
            if (result == null)
                return result;

            result[0] = 0x55;
            result[1] = 0xAA;
            return result;
        }

        /// <summary>
        /// Convert to current instance byte array (for sending through ethernet)
        /// Might return null.
        /// </summary>
        public byte[] ToByteArrayDown()
        {
            var result = ToByteArray();
            if (result == null)
                return result;

            result[0] = 0xAA;
            result[1] = 0x55;
            return result;
        }

        /// <summary>
        /// Message must have valid card type and payload
        /// </summary>
        /// <returns></returns>
        public bool CheckValid()
        {
            //if ((HeaderBytes == null) || (HeaderBytes.Length != _payloadOffset))
            //    return false;

            // card type exists
            if (!ProjectRegistry.Instance.DeviceKeys.ContainsKey(CardType))
                return false;

            // payload 
            if ((PayloadBytes==null) || (PayloadBytes.Length == 0))
                return false;

            return true;
        }
        /// <summary>
        /// Create EthernetMessage from byte array
        /// </summary>
        public static EthernetMessage CreateFromByteArray(byte[] byteMessage, out string errorString)
        {
            EthernetMessage resutlMessage = null;
            if (false == CheckByteArrayValidity(byteMessage, out errorString))
            {
                return null;
            }


            // Build message
            // ============
            resutlMessage = new EthernetMessage();
            // header & payload
            resutlMessage.HeaderBytes = new byte[EthernetMessage.PayloadOffset];
            Array.Copy(byteMessage, resutlMessage.HeaderBytes, EthernetMessage.PayloadOffset);
            resutlMessage.PayloadBytes = new byte[byteMessage.Length - EthernetMessage.PayloadOffset];
            Array.Copy(byteMessage, EthernetMessage.PayloadOffset, resutlMessage.PayloadBytes, 0, byteMessage.Length - EthernetMessage.PayloadOffset);

            // type & slot
            resutlMessage.CardType = byteMessage[EthernetMessage.CardTypeOffset];
            resutlMessage.SlotChannelNumber = byteMessage[EthernetMessage.SlotChannelNumberOffset];

            return resutlMessage;
        }

        private static bool CheckByteArrayValidity(byte[] byteMessage, out string errorString)
        {
            errorString  = null;
            bool rc = false;
            // min len
            if (byteMessage.Length < 16)
            {
                errorString = $"Byte message too short {byteMessage.Length}";
                goto exit;
            }
            // preamble
            if (byteMessage[0] != 0xAA || byteMessage[1] != 0x55)
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

            rc = true;

        exit: return rc;
        }

        /// <summary>
        /// Create EthernetMessage from device result.
        /// Might return null.
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="deviceString"></param>
        public static EthernetMessage CreateFromDevice(byte[] payload, string deviceString)
        {
            //ILog _logger = log4net.LogManager.GetLogger("Root");

            int key = ProjectRegistry.Instance.GetDeviceKeyFromDeviceString(deviceString);
            if (key < 0)
            {
                //_logger.Warn($"Unknown device string {deviceString}");
                return null;
            }

            EthernetMessage msg = new EthernetMessage();
            msg.CardType = (byte)key;
            msg.PayloadBytes = payload;

            System.Diagnostics.Debug.Assert(msg.CheckValid());

            return msg;
        }

        public static EthernetMessage CreateEmpty(int cardType, int payloadLength)
        {
            EthernetMessage msg = new EthernetMessage();
            msg.HeaderBytes = new byte[EthernetMessage.PayloadOffset];
            msg.PayloadBytes = new byte[payloadLength];
            msg.CardType = cardType;
            return msg;
        }

    }

}
