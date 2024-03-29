﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace UeiBridge.Library
{
    public enum MessageWay { upstream, downstream}
    /// <summary>
    /// This class represents an icd dedicated datagram 
    /// that might be received or sent by this application
    /// </summary>
    public class EthernetMessage
    {
        //const int _payloadOffset = 16;
        //const int _cardTypeOffset = 5; // 1 byte
        //const int _lengthOffset = 12; // 2 bytes

        public int CubeId { get; set; }
        public int CardType { get; set; }
        public int SlotNumber { get; set; }
        public int SerialChannelNumber { get; set; }
        [JsonProperty("Payload")]
        public byte[] PayloadBytes { get; set; }
        //public List<byte> SomeBytes { get; set; } 
        //public byte[] HeaderBytes { get; set; }
        //public int _debugSerial { get; set; } // serial number of message
        [JsonIgnore]
        public int TotalLength => (_payloadOffset + PayloadBytes.Length);

        public const int _payloadOffset = 16;// _payloadOffset;
        public const int _unitIdOffset = 4;
        public const int _cardTypeOffset = 5;// _cardTypeOffset;
        public const int _slotNumberOffset = 6;
        public const int _serialChannelOffset = 7;
        public const int _lengthOffset = 12;// _lengthOffset;

        /// <summary>
        /// Convert to current instance byte array (for sending through ethernet)
        /// Might return null.
        /// </summary>
        byte[] ToByteArray()
        {
            if (!InternalValidityTest())
                return null;

            byte[] messageBytes = new byte[_payloadOffset + PayloadBytes.Length];
            Array.Clear(messageBytes, 0, messageBytes.Length);

            messageBytes[_unitIdOffset] = (byte)CubeId;
            messageBytes[_cardTypeOffset] = (byte)CardType;
            messageBytes[_serialChannelOffset] = (byte)SerialChannelNumber;
            messageBytes[_slotNumberOffset] = (byte)SlotNumber;

            // message length
            byte[] twobytes = BitConverter.GetBytes(messageBytes.Length);
            Array.Copy(twobytes, 0, messageBytes, _lengthOffset, twobytes.Length);

            // payload
            Array.Copy(PayloadBytes, 0, messageBytes, _payloadOffset, PayloadBytes.Length);

            return messageBytes;
        }
        /// <summary>
        /// Convert to current instance byte array (for sending through ethernet)
        /// Might return null.
        /// </summary>
        public byte[] GetByteArray( MessageWay way)
        {
            var result = ToByteArray();
            if (result == null)
                return result;

            if (way == MessageWay.upstream)
            {
                result[0] = 0x55;
                result[1] = 0xAA;
            }
            else
            {
                result[0] = 0xAA;
                result[1] = 0x55;
            }

            return result;
        }

        /// <summary>
        /// Internal Validity Test
        /// </summary>
        /// <returns></returns>
        public bool InternalValidityTest()
        {
            // payload 
            if ((PayloadBytes==null) || (PayloadBytes.Length == 0))
                return false;

            //if (PayloadBytes.Length != NominalLength-_payloadOffset)
            //{
            //    return false;
            //}
            return true;
        }
        /// <summary>
        /// Create EthernetMessage from byte array
        /// </summary>
        //public static EthernetMessage CreateFromByteArray(byte[] byteMessage, MessageWay way, ref string errorString)
        public static EthernetMessage CreateFromByteArray(byte[] byteMessage, MessageWay way, Action<string> onError)
        {
            if (null==byteMessage)
            {
                return null;
            }

            EthernetMessage resutlMessage = null;
            //errorString = null;
            string errMsg;
            if (false == CheckByteArrayValidity(byteMessage, way, out errMsg))
            {
                onError?.Invoke(errMsg);
                return null;
            }

            // Build message struct
            // ====================
            resutlMessage = new EthernetMessage();
            // header & payload
            //resutlMessage.HeaderBytes = new byte[EthernetMessage._payloadOffset];
            //Array.Copy(byteMessage, resutlMessage.HeaderBytes, EthernetMessage._payloadOffset);
            resutlMessage.PayloadBytes = new byte[byteMessage.Length - _payloadOffset];
            Array.Copy(byteMessage, _payloadOffset, resutlMessage.PayloadBytes, 0, byteMessage.Length - EthernetMessage._payloadOffset);

            resutlMessage.CubeId = byteMessage[_unitIdOffset];
            resutlMessage.CardType = byteMessage[_cardTypeOffset];
            resutlMessage.SerialChannelNumber = byteMessage[_serialChannelOffset];
            resutlMessage.SlotNumber = byteMessage[_slotNumberOffset];
            //resutlMessage.NominalLength = byteMessage.Length; 

            return resutlMessage;
        }

        private static bool CheckByteArrayValidity(byte[] byteMessage, MessageWay way, out string errorString)
        {
            errorString  = null;
            bool rc = false;
            // min len
            if (byteMessage.Length < 16)
            {
                errorString = $"Message too short {byteMessage.Length}";
                goto exit;
            }
            // preamble
            if (way == MessageWay.downstream)
            {
                if (byteMessage[0] != 0xAA || byteMessage[1] != 0x55)
                {
                    errorString = $"Wrong preamble";
                    goto exit;
                }
            }
            else // upstream
            {
                if (byteMessage[0] != 0x55 || byteMessage[1] != 0xAA)
                {
                    errorString = $"Wrong preamble";
                    goto exit;
                }
            }
            UInt16 nominalLengh = BitConverter.ToUInt16(byteMessage, EthernetMessage._lengthOffset);
            if (nominalLengh != byteMessage.Length)
            {
                errorString = $"Inconsistent length declared:{nominalLengh}  actual:{byteMessage.Length}";
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
        //[Obsolete]
        //public static EthernetMessage CreateFromDevice(byte[] payload, DeviceSetup setup, int serialChannel=0)
        //{
        //    //ILog _logger = log4net.LogManager.GetLogger("Root");

        //    //int key = //ProjectRegistry.Instance.GetDeviceKeyFromDeviceString(deviceName);
        //    int key = StaticMethods.GetCardIdFromCardName( setup.DeviceName);

        //    System.Diagnostics.Debug.Assert(key >= 0);

        //    EthernetMessage msg = new EthernetMessage();
        //    if (setup.GetType() == typeof(SL508892Setup))
        //    {
        //        msg.SerialChannelNumber = serialChannel;
        //    }

        //    msg.SlotNumber = setup.SlotNumber;
        //    msg.UnitId = 0; // 
        //    msg.CardType = (byte)key;
        //    msg.PayloadBytes = payload;

        //    System.Diagnostics.Debug.Assert(msg.CheckValid());

        //    return msg;
        //}

        public static EthernetMessage CreateEmpty(int cardType, int payloadLength)
        {
            EthernetMessage msg = new EthernetMessage();
            //msg.HeaderBytes = new byte[EthernetMessage._payloadOffset];
            msg.PayloadBytes = new byte[payloadLength];
            msg.CardType = cardType;
            return msg;
        }

        public static EthernetMessage CreateMessage(int cardId, int slotNumber, int unitId, byte[] payload)
        {
            EthernetMessage msg = new EthernetMessage();
            msg.PayloadBytes = payload;
            msg.CardType = cardId;
            msg.SlotNumber = slotNumber;
            msg.CubeId = unitId;
            //msg.SerialChannelNumber = serialChNumber;
            //msg.NominalLength = payload.Length + _payloadOffset;

            //msg.SomeBytes = new List<byte>(payload);

            return msg;
        }

    }

}
