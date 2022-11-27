using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UeiBridge
{
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
            //if (!CheckValid())
            //    return null;

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
#if dont
        public bool CheckValid()
        {
            //if ((HeaderBytes == null) || (HeaderBytes.Length != _payloadOffset))
            //    return false;

            // card type exists
            if (!ProjectRegistry.Instance.DeviceKeys.ContainsKey(CardType))
                return false;

            // payload 
            if ((PayloadBytes == null) || (PayloadBytes.Length == 0))
                return false;

            return true;
        }

#endif 
    }

    }
