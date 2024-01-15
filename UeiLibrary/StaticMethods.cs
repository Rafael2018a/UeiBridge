// Ignore Spelling: Uei

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UeiDaq;

namespace UeiBridge.Library
{
    // Methods in the class SHOULD NOT depend on any other project class
    public class StaticMethods
    {
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
        public static byte[] Make_Dio403_upstream_message(byte[] payload)
        {
            int id = DeviceMap2.GetDeviceName(DeviceMap2.DIO403Literal);
            var b = EthernetMessage.CreateMessage(id, 0, 0, payload); //new byte[] { 0x5, 0, 0 });
            return b.GetByteArray(MessageWay.upstream);
        }

        public static EthernetMessage Make_BlockSensor_downstream_message(Int16[] payload)
        {
            if (payload.Length != 14) throw new ArgumentException();

            byte[] result = new byte[payload.Length * 2];
            for (int i = 0; i < payload.Length; i++)
            {
                byte[] two = BitConverter.GetBytes(payload[i]);
                result[i * 2] = two[0];
                result[i * 2 + 1] = two[1];
            }

            return EthernetMessage.CreateMessage(32, 0, 2, result);
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

        public static List<UeiDeviceInfo> DeviceCollectionToDeviceInfoList_old(DeviceCollection devColl, string cubeurl)
        {
            try
            {
                var l1 = devColl.Cast<UeiDaq.Device>().ToList();
                var devList = l1.Select((UeiDaq.Device i) =>
                {
                    if (i == null)
                        return null;
                    if (i.GetDeviceName().ToLower().StartsWith("dnrp"))
                        return null;
                    //Uri url = new Uri(i.GetResourceName());
                    //string curl = url.LocalPath;
                    return new UeiDeviceInfo(cubeurl, i.GetIndex(), i.GetDeviceName());
                });
                var l2 = devList.ToList();
                //l2.Remove(null); // remove last null item
                l2.RemoveAll(StaticMethods.IsNullPredicate);
                return l2;
            }
            catch (UeiDaq.UeiDaqException)
            {
                return null;
            }
        }
#if old
        public static int GetCubeId_old(string cubeUrl)
        {
            int result = -1;
            if (null != cubeUrl)
            {
                if (cubeUrl.ToLower().StartsWith("simu"))
                {
                    result = UeiDeviceInfo.SimuCubeId;
                }
                else
                {
                    IPAddress ipa = CubeUrlToIpAddress(cubeUrl);
                    if (null != ipa)
                    {
                        result = ipa.GetAddressBytes()[3];
                    }
                }
            }
            return result;
        }
#endif
        public static int GetCubeId_old(IPAddress cubeIp)
        {
            if (null == cubeIp)
            {
                return -1;
            }
            else
            {
                return cubeIp.GetAddressBytes()[3];
            }    
        }
        public static string GetCubeUrl_old(IPAddress ip)
        {
            if (null==ip)
            {
                return null;
            }
            StringBuilder sb = new StringBuilder("pdna://");
            sb.Append(ip.ToString());
            sb.Append("/");
            return sb.ToString();
        }

        public static System.Net.IPAddress CubeUrlToIpAddress_old(string url)
        {
            //m.Net.IPAddress result = null;
            Uri resutlUri;
            bool ok1 = Uri.TryCreate(url, UriKind.Absolute, out resutlUri);
            if (ok1)
            {
                IPAddress resutlIp;
                bool ok2 = System.Net.IPAddress.TryParse(resutlUri.Host, out resutlIp);
                if (ok2)
                {
                    return resutlIp;
                }
            }
            return null;
        }

        public static bool IsNullPredicate(object u)
        {
            return (u == null) ? true : false;
        }

        public static System.Reflection.Assembly GetLibVersion()
        {
            //System.Reflection.Assembly EntAsm = System.Reflection.Assembly.GetEntryAssembly();
            //System.Reflection.Assembly asm1 = System.Reflection.Assembly.GetCallingAssembly();
            //System.Reflection.Assembly asm2 = System.Reflection.Assembly.GetEntryAssembly();
            System.Reflection.Assembly asm3 = System.Reflection.Assembly.GetExecutingAssembly();

            return asm3;
        }

        public static List<IPAddress> GetLocalIpList()
        {
            List<IPAddress> result = new List<IPAddress>();
            foreach (IPAddress ip in Dns.GetHostAddresses(Dns.GetHostName()))
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    result.Add(ip);
                }
            }
            return result;
        }
    }
}
