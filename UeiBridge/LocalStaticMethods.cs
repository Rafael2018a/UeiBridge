using System;
using System.Collections.Generic;
using UeiBridge.Library;

namespace UeiBridge
{
    public static class LocalStaticMethods
    {
        public static byte[] Make_A308Down_message()
        {
            EthernetMessage msg = EthernetMessage.CreateEmpty(0, 16);
            for (int i = 0; i < 16; i += 2)
            {
                msg.PayloadBytes[i] = (byte)(i * 10);
            }
            return msg.ToByteArrayDown();
        }
        public static byte[] Make_DIO403Down_Message()
        {
            EthernetMessage msg = EthernetMessage.CreateEmpty(4, 3);
            msg.PayloadBytes[0] = 0x12;
            msg.PayloadBytes[1] = 0x34;
            msg.PayloadBytes[2] = 0x56;
            msg.SlotNumber = 5;

            return msg.ToByteArrayDown();
        }
        public static byte[] Make_DIO470_Down_Message()
        {
            EthernetMessage msg = EthernetMessage.CreateEmpty(6, 3);
            msg.PayloadBytes[0] = 0x12;
            msg.PayloadBytes[1] = 0x34;
            msg.PayloadBytes[2] = 0x56;
            msg.SlotNumber = 4;

            return msg.ToByteArrayDown();
        }
        public static byte[] Make_DIO430Down_Message()
        {
            EthernetMessage msg = EthernetMessage.CreateEmpty(6, 16);
            return msg.ToByteArrayDown();
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
                msgs.Add(msg.ToByteArrayDown());
            }
            return msgs;
        }


        /// <summary>
        /// Find and instantiate suitalble converter
        /// </summary>
        /// <returns></returns>
        public static IConvert CreateConverterInstance( DeviceSetup setup) // tbd. deviceName not needed
        {
            IConvert attachedConverter = null;
            foreach (Type theType in System.Reflection.Assembly.GetExecutingAssembly().GetTypes())
            {
                if (theType.IsInterface || theType.IsAbstract)
                    continue;

                if (typeof(IConvert).IsAssignableFrom(theType))
                {
                    var at = (IConvert)Activator.CreateInstance(theType, setup);
                    if (at.DeviceName == setup.DeviceName)
                    {
                        attachedConverter = at;
                        break;
                    }
                }
            }
            
            return attachedConverter;
        }

        /// <summary>
        /// Create EthernetMessage from device result.
        /// Might return null.
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="deviceString"></param>
        //[Obsolete]
        public static EthernetMessage CreateEthernetMessage(byte[] payload, DeviceSetup setup, int serialChannel = 0)
        {
            //ILog _logger = log4net.LogManager.GetLogger("Root");

            //int key = //ProjectRegistry.Instance.GetDeviceKeyFromDeviceString(deviceName);
            int key = StaticMethods.GetCardIdFromCardName(setup.DeviceName);

            System.Diagnostics.Debug.Assert(key >= 0);

            EthernetMessage msg = new EthernetMessage();
            if (setup.GetType() == typeof(SL508892Setup))
            {
                msg.SerialChannelNumber = serialChannel;
            }

            msg.SlotNumber = setup.SlotNumber;
            msg.UnitId = 0; // tbd
            msg.CardType = (byte)key;
            msg.PayloadBytes = payload;

            System.Diagnostics.Debug.Assert(msg.CheckValid());

            return msg;
        }

    }
}
