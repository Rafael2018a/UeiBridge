// Ignore Spelling: Uei

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UeiBridge.Library.CubeSetupTypes;
using UeiDaq;

namespace UeiBridge.Library
{
    // Methods in the class SHOULD NOT depend on any other project class
    public static class StaticMethods
    {
        static Dictionary<SerialPortSpeed, int> _serialSpeedDic = new Dictionary<SerialPortSpeed, int>();

        static StaticMethods()
        {
            _serialSpeedDic.Add(SerialPortSpeed.BitsPerSecond110, 110);
            _serialSpeedDic.Add(SerialPortSpeed.BitsPerSecond300, 300);
            _serialSpeedDic.Add(SerialPortSpeed.BitsPerSecond600, 600);
            _serialSpeedDic.Add(SerialPortSpeed.BitsPerSecond1200, 1200);
            _serialSpeedDic.Add(SerialPortSpeed.BitsPerSecond2400, 2400);
            _serialSpeedDic.Add(SerialPortSpeed.BitsPerSecond4800, 4800);
            _serialSpeedDic.Add(SerialPortSpeed.BitsPerSecond9600, 9600);
            _serialSpeedDic.Add(SerialPortSpeed.BitsPerSecond14400, 14400);
            _serialSpeedDic.Add(SerialPortSpeed.BitsPerSecond19200, 19200);
            _serialSpeedDic.Add(SerialPortSpeed.BitsPerSecond28800, 28800);
            _serialSpeedDic.Add(SerialPortSpeed.BitsPerSecond38400, 38400);
            _serialSpeedDic.Add(SerialPortSpeed.BitsPerSecond57600, 57600);
            _serialSpeedDic.Add(SerialPortSpeed.BitsPerSecond115200, 115200);
            _serialSpeedDic.Add(SerialPortSpeed.BitsPerSecond128000, 128000);
            _serialSpeedDic.Add(SerialPortSpeed.BitsPerSecond250000, 250000);
            _serialSpeedDic.Add(SerialPortSpeed.BitsPerSecond256000, 256000);
            _serialSpeedDic.Add(SerialPortSpeed.BitsPerSecond1000000, 1000000);

        }
        public static int GetSerialSpeedAsInt( SerialPortSpeed speed)
        {
            int result;
            if (_serialSpeedDic.TryGetValue(speed, out result))
            {
                return result;
            }
            else
            {
                return -1;
            }
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
        public static byte[] Make_Dio403_upstream_message(byte[] payload)
        {
            int id = DeviceMap2.GetDeviceIdFromName(DeviceMap2.DIO403Literal);
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

        /// <summary>
        /// Create EthernetMessage from device result.
        /// Might return null.
        /// </summary>
        public static EthernetMessage BuildEthernetMessageFromDevice(byte[] payload, DeviceSetup setup, int serialChannel = 0)
        {
            //ILog _logger = log4net.LogManager.GetLogger("Root");

            //int key = //ProjectRegistry.Instance.GetDeviceKeyFromDeviceString(deviceName);
            int key = DeviceMap2.GetDeviceIdFromName(setup.DeviceName);

            System.Diagnostics.Debug.Assert(key >= 0);

            EthernetMessage msg = new EthernetMessage();
            if (setup.GetType() == typeof(SL508892Setup))
            {
                msg.SerialChannelNumber = serialChannel;
            }

            msg.SlotNumber = setup.SlotNumber;
            msg.CubeId = 0;
            msg.CardType = (byte)key;
            msg.PayloadBytes = payload;
            //msg.NominalLength = payload.Length + EthernetMessage._payloadOffset;

            System.Diagnostics.Debug.Assert(msg.InternalValidityTest());

            return msg;
        }

        public static int Index2Of(byte[] data, byte[] pattern, int startIndex)
        {
            //if (data == null) throw new ArgumentNullException(nameof(data));
            //if (pattern == null) throw new ArgumentNullException(nameof(pattern));
            System.Diagnostics.Debug.Assert(pattern.Length == 2);

            var cycles = data.Length - pattern.Length + 1;
            long patternIndex;
            for (var dataIndex = startIndex + pattern.Length; dataIndex < cycles; dataIndex++)
            {
                if (data[dataIndex] != pattern[0])
                    continue;
                for (patternIndex = pattern.Length - 1; patternIndex >= 1; patternIndex--)
                {
                    if (data[dataIndex + patternIndex] != pattern[patternIndex])
                        break;
                }
                if (patternIndex == 0)
                    return dataIndex;
            }
            return -1; // pattern not found
        }
        /// <summary>
        /// Find pattern in 'data'
        /// If pattern not found, return full message length
        /// If startIndex is greater then message length, return -1;
        /// </summary>
        /// <param name="data"></param>
        /// <param name="pattern"></param>
        /// <param name="startIndex"></param>
        /// <returns>Location of pattern</returns>
        public static int IndexOf(byte[] data, byte[] pattern, int startIndex)
        {
            if (startIndex >= data.Length)
                return -1;
            //if (data == null) throw new ArgumentNullException(nameof(data));
            //if (pattern == null) throw new ArgumentNullException(nameof(pattern));
            System.Diagnostics.Debug.Assert(pattern.Length == 2);

            var cycles = data.Length - pattern.Length + 1;
            for (var dataIndex = startIndex; dataIndex < cycles; dataIndex++)
            {
                if (data[dataIndex] != pattern[0])
                    continue;
                if (data[dataIndex + 1] != pattern[1])
                    continue;
                return dataIndex;
            }
            return data.Length;
        }
    }
}
