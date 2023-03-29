﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UeiBridge.Library
{
    // Methods in the class SHOULD NOT depend on any other project class
    public class StaticMethods
    {
        public static System.Net.IPAddress GetIpAddressFromUrl(string url)
        {
            Uri u1 = new Uri(url);
            var a1 = u1.Host;
            System.Net.IPAddress result;
            try
            {
                result = System.Net.IPAddress.Parse(u1.Host);
            }
            catch (FormatException ex)
            {
                result = System.Net.IPAddress.None;
            }
            return result;
        }
        public static string GetEnumValues<T>()
        {
            T[] v1 = Enum.GetValues(typeof(T)) as T[];
            StringBuilder sb = new StringBuilder("\n");
            foreach (var item in v1)
            {
                sb.Append(item);
                sb.Append("\n");
            }
            return sb.ToString();
        }

        public static byte[] Make_A308Down_message()
        {
            EthernetMessage msg = EthernetMessage.CreateEmpty(0, 16);
            for (int i = 0; i < 16; i += 2)
            {
                msg.PayloadBytes[i] = (byte)(i);
            }
            return msg.GetByteArray(MessageWay.downstream);
        }
        public static byte[] Make_DIO403Down_Message()
        {
            EthernetMessage msg = EthernetMessage.CreateEmpty(4, 3);
            msg.PayloadBytes[0] = 0x12;
            msg.PayloadBytes[1] = 0x34;
            msg.PayloadBytes[2] = 0x56;
            msg.SlotNumber = 5;

            return msg.GetByteArray(MessageWay.downstream);
        }
        public static byte[] Make_Dio403_upstream_message()
        {
            int id = DeviceMap.GetCardIdFromCardName("DIO-403");
            var b = EthernetMessage.CreateMessage(id, 0, 0, new byte[] { 0x5, 0, 0 });
            return b.GetByteArray(MessageWay.upstream);
        }

        public static EthernetMessage Make_BlockSensor_downstream_message(UInt16[] payload)
        {
            if (payload.Length != 14) throw new ArgumentException();

            byte[] result = new byte[payload.Length * 2];
            for (int i = 0; i < payload.Length; i++)
            {
                byte[] two = BitConverter.GetBytes(payload[i]);
                result[i * 2] = two[0];
                result[i * 2 + 1] = two[1];
            }

            return EthernetMessage.CreateMessage(32, -1, 0, result);
        }


        public static byte[] Make_DIO470_Down_Message()
        {
            EthernetMessage msg = EthernetMessage.CreateEmpty(6, 3);
            msg.PayloadBytes[0] = 0x12;
            msg.PayloadBytes[1] = 0x34;
            msg.PayloadBytes[2] = 0x56;
            msg.SlotNumber = 4;

            return msg.GetByteArray(MessageWay.downstream);
        }
        public static byte[] Make_DIO430Down_Message()
        {
            EthernetMessage msg = EthernetMessage.CreateEmpty(6, 16);
            return msg.GetByteArray(MessageWay.downstream);
        }
        public static List<byte[]> Make_SL508Down_Messages(int seed)
        {
            List<byte[]> msgs = new List<byte[]>();

            // build 8 messages, one per channel
            for (int ch = 0; ch < 8; ch++)
            {
                string m = $"hello ch{ch} seed {seed} ------------ ";

                // string to ascii

                // ascii to string System.Text.Encoding.ASCII.GetString(recvBytes)
                EthernetMessage msg = EthernetMessage.CreateEmpty(cardType: 5, payloadLength: 16);
                msg.PayloadBytes = System.Text.Encoding.ASCII.GetBytes(m);
                msg.SerialChannelNumber = ch;
                msg.SlotNumber = 3;
                msgs.Add(msg.GetByteArray(MessageWay.downstream));
            }
            return msgs;
        }

    }
}