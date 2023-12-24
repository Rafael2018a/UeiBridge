using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UeiDaq;

namespace UeiBridge.Library
{

    public class UeiCube
    {
        IPAddress CubeAddress { get; set; }
        public UeiCube(string cubeUri)
        {
            CubeAddress = CubeUriToIpAddress(cubeUri);

            if (null == this.CubeAddress)
            {
                throw new ArgumentNullException();
            }

        }
        public UeiCube(IPAddress cubeAddress)
        {
            if (null == this.CubeAddress)
            {
                throw new ArgumentNullException();
            }

            this.CubeAddress = cubeAddress;
        }

        public string GetCubeUri()
        {
            StringBuilder sb = new StringBuilder("pdna://");
            sb.Append( this.CubeAddress.ToString());
            sb.Append("/");
            return sb.ToString();
        }
        /// <summary>
        /// (DNRP device NOT included)
        /// </summary>
        /// <returns></returns>
        public List<UeiDeviceInfo> GetDeviceInfoList()
        {
            DeviceCollection devColl = new DeviceCollection( this.GetCubeUri());

            try
            {
                List<Device>  l1 = devColl.Cast<UeiDaq.Device>().ToList();
                var devList = l1.Select((UeiDaq.Device i) =>
                {
                    if (i == null)
                        return null;
                    if (i.GetDeviceName().ToLower().StartsWith("dnrp"))
                        return null;
                    return new UeiDeviceInfo(this.GetCubeUri(), i.GetIndex(), i.GetDeviceName());
                });
                List<UeiDeviceInfo> l2 = devList.ToList();
                l2.RemoveAll(StaticMethods.IsNullPredicate);// remove last null item
                return l2;
            }
            catch (UeiDaq.UeiDaqException)
            {
                return null;
            }
        }

        public Device GetDevice(int slotNumber) 
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Device.Reset() doc: Executes a hardware reset on the device. To reboot a PowerDNA or PowerDNR unit call this method on the CPU device (device 14). 
        /// </summary>
        public void CubeReboot()
        {
            //var ip = UeiBridge.Library.StaticMethods.CubeUrlToIpAddress(cubeUri);
            throw new NotImplementedException();
        }

        /// <summary>
        /// Reset dnrp
        /// </summary>
        /// <param name="cubeAddress"></param>
        public void CubeReset()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="slotNumber">One of the cards, not DNRP and not 'dev14'</param>
        public void DeviceReset(uint slotNumber)
        {
            throw new NotImplementedException();
        }
        public void DeviceResetAll()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Formerly, TryIp
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool IsCubeConnected(IPAddress ipAddress)
        {
            UdpClient udpClient = new UdpClient();
            try
            {
                Byte[] sendBuffer = new byte[255];
                BinaryWriter writer = new BinaryWriter(new MemoryStream(sendBuffer));

                writer.Write((uint)IPAddress.HostToNetworkOrder(unchecked((int)0xbabafaca)));
                writer.Write((ushort)IPAddress.HostToNetworkOrder(0));
                writer.Write((ushort)IPAddress.HostToNetworkOrder(0));
                writer.Write((uint)IPAddress.HostToNetworkOrder(unchecked((int)0x104)));
                writer.Write((uint)IPAddress.HostToNetworkOrder(0));

                IPEndPoint ep = new IPEndPoint(ipAddress, 6334);
                udpClient.Send(sendBuffer, (int)writer.BaseStream.Length, ep);

                udpClient.Client.ReceiveTimeout = 500;
                Byte[] recvBuffer = udpClient.Receive(ref ep);
                BinaryReader reader = new BinaryReader(new MemoryStream(recvBuffer));
                uint prolog = (uint)IPAddress.NetworkToHostOrder(unchecked((int)reader.ReadUInt32()));
                ushort ts = (ushort)IPAddress.NetworkToHostOrder(unchecked((short)reader.ReadUInt16()));
                ushort cnt = (ushort)IPAddress.NetworkToHostOrder(unchecked((short)reader.ReadUInt16()));
                uint cmd = (uint)IPAddress.NetworkToHostOrder(unchecked((int)reader.ReadUInt32()));
                uint reqid = (uint)IPAddress.NetworkToHostOrder(unchecked((int)reader.ReadUInt32()));

                if (cmd == 0x1104)
                {
                    return true;
                }
                else
                    return false;
            }
            catch (SocketException)
            {
                return false;
            }
        }

        public void DasReset1(string cubeuri)
        {
            int deviceNumber;
            int powerDev = 0;
            Device myDevice;
            string devCollName;


            DeviceCollection devColl = new DeviceCollection("pdna://192.168.100.40");

            foreach (Device dev in devColl)
            {
                if (dev != null)
                {
                    devCollName = dev.GetDeviceName();

                    if (devCollName == "DNRP-40")
                    {
                        powerDev = dev.GetIndex();
                    }
                }
            }


            for (deviceNumber = 0; deviceNumber <= powerDev - 2; deviceNumber++)
            {
                string rNAME = @"pdna://192.168.100.40/Dev" + deviceNumber.ToString().Trim() + @"/";
                myDevice = new Device();
                myDevice = DeviceEnumerator.GetDeviceFromResource(rNAME);
                string devName = myDevice.GetDeviceName();
                if (myDevice != null)
                {
                    myDevice.Reset();
                }
            }



        }
        public void DasReset2(string cubeuri)
        {

            string sDevCollName = "";
            Device myDevice;
            string pdnaIpAddress = "pdna://192.168.100.50";
            int nDeviceNumber = 0;
            DeviceCollection devColl = new DeviceCollection(pdnaIpAddress);

            DateTime localdatestartloop = System.DateTime.Now;
            System.Console.WriteLine("Before Loop Date/Time: {0}", localdatestartloop.ToString("yyyy-MM-dd HH:mm:ss:FFF"));

            foreach (Device dev in devColl)
            {
                if (dev != null)
                {
                    sDevCollName = dev.GetDeviceName();
                    int myDevNumber = dev.GetIndex();
                    Console.WriteLine(sDevCollName);

                    // 2020-04-23 JED - added per Brian Dao from UEI to only reset certain layers
                    string[] sDevCollNameSplit = sDevCollName.Split('-');
                    string sModel = sDevCollNameSplit[1];
                    int nModelNum = Convert.ToInt16(sModel);
                    if (nModelNum > 100)
                    {
                        string sName = pdnaIpAddress + "/dev" + nDeviceNumber.ToString().Trim() + "/";
                        myDevice = DeviceEnumerator.GetDeviceFromResource(sName);
                        if (myDevice != null)
                        {
                            Console.WriteLine("Reset");
                            myDevice.Reset();
                        }
                    }
                }
                nDeviceNumber++;
            }
            DateTime localdatestoploop = System.DateTime.Now;
            System.Console.WriteLine("After Loop Date/Time: {0}", localdatestoploop.ToString("yyyy-MM-dd HH:mm:ss:FFF"));


        }

        internal int GetCubeId()
        {
            if (null == CubeAddress)
            {
                return -1;
            }
            else
            {
                return CubeAddress.GetAddressBytes()[3];
            }
        }

        internal string GetCubeUri(IPAddress cubeIp)
        {
            throw new NotImplementedException();
        }

        public static System.Net.IPAddress CubeUriToIpAddress(string uri)
        {
            Uri resutlUri;
            bool ok1 = Uri.TryCreate(uri, UriKind.Absolute, out resutlUri);
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

    }
}
