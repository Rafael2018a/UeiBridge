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

    /// <summary>
    /// Cube management class.
    /// - check cube exist (connected)
    /// - get list of devices in cube
    /// - get/convert cube id/ip/url
    /// - reset specific device, 
    /// - reset whole cube (Watchdog reset)
    /// </summary>
    public class UeiCube
    {
        const string _simuUri = "simu://";

        public IPAddress CubeAddress { get; private set; }
        public bool IsSimuCube { get; private set; } = false;
        public bool IsValidAddress { get; private set; } = false;
        public UeiCube(string cubeUri)
        {
            System.Diagnostics.Debug.Assert(cubeUri != null);
            if (cubeUri.ToLower().StartsWith("simu"))
            {
                IsSimuCube = true;
                IsValidAddress = true;
                return;
            }

            IPAddress ip;
            if (IPAddress.TryParse(cubeUri, out ip))
            {
                this.CubeAddress = ip;
                IsSimuCube = false;
                IsValidAddress = true;
                return;
            }

            CubeAddress = CubeUriToIpAddress(cubeUri);
            IsValidAddress = (null == this.CubeAddress) ? false : true;
            IsSimuCube = false;
        }

        public UeiCube(IPAddress cubeAddress)
        {
            System.Diagnostics.Debug.Assert(null != cubeAddress);
            this.CubeAddress = cubeAddress;
            IsSimuCube = false;
            IsValidAddress = true;
        }

        /// <summary>
        /// Cube id is the fourth field of the cube ip
        /// </summary>
        /// <returns></returns>
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

        public string GetCubeUri()
        {
            if (this.IsSimuCube)
            {
                return _simuUri;
            }
            if (null==this.CubeAddress)
            {
                return null;
            }

            StringBuilder sb = new StringBuilder("pdna://");
            sb.Append( this.CubeAddress.ToString());
            sb.Append("/");
            return sb.ToString();
        }

        /// <summary>
        /// Get real device info list
        /// (DNRP device NOT included)
        /// </summary>
        /// <returns></returns>
        public List<UeiDeviceInfo> GetDeviceInfoList()
        {
            if (false == IsValidAddress)
            {
                return null;
            }

            DeviceCollection devColl = new DeviceCollection(this.GetCubeUri());

            try
            {
                List<Device> l1 = devColl.Cast<UeiDaq.Device>().ToList();
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

        public List<Device> GetDeviceList()
        {
            if (false == IsValidAddress && IsSimuCube==false)
            {
                return null;
            }
            
            try
            {
                DeviceCollection devColl = new DeviceCollection(this.GetCubeUri());
                List<Device> l1 = devColl.Cast<UeiDaq.Device>().ToList();
                // remove null entries
                var l2 = l1.Where(i => i != null);
                var l3 = l2.Cast<UeiDaq.Device>().ToList();
                return l3;
            }
            catch (UeiDaq.UeiDaqException)
            {
                return null;
            }
        }

        public Device GetDevice(int slotNumber) 
        {
            string devuri = $"{this.GetCubeUri()}Dev{slotNumber}";
            return DeviceEnumerator.GetDeviceFromResource(devuri);
        }

        public bool DeviceReset( string deviceUri)
        {
            if (this.IsSimuCube)
            {
                return true;
            }
            Device dev = DeviceEnumerator.GetDeviceFromResource(deviceUri);
            try
            {
                if (dev != null)
                {
                    dev.Reset();
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch(Exception ex)
            {
                return false;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// From doc: Device.Reset() Executes a hardware reset on the device. To reboot a PowerDNA or PowerDNR unit call this method on the CPU device (device 14). 
        /// Rafi: Might as well use "cpu" instead of "Dev14", see Diagnostics project in Uei-Examples
        /// </remarks>
        public bool CubeReset()
        {
            string cpudev = $"{this.GetCubeUri()}cpu"; 
            return this.DeviceReset(cpudev);
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
        public bool IsCubeConnected()//IPAddress ipAddress)
        {
            if (false==IsValidAddress)
            {
                return false;
            }
            if (true==IsSimuCube)
            {
                return true;
            }

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

                IPEndPoint ep = new IPEndPoint(CubeAddress, 6334);
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

        public void DasReset1()
        {
            int deviceNumber;
            int powerDev = 0;
            Device myDevice;
            string devCollName;
            string cubeUri = this.GetCubeUri();

            DeviceCollection devColl = new DeviceCollection(cubeUri);

            foreach (Device dev in devColl)
            {
                if (dev != null)
                {
                    devCollName = dev.GetDeviceName();

                    if (devCollName.ToLower().StartsWith("dnrp"))
                    {
                        powerDev = dev.GetIndex();
                    }
                }
            }


            for (deviceNumber = 0; deviceNumber <= powerDev - 2; deviceNumber++)
            {
                string rNAME = cubeUri + "Dev" + deviceNumber.ToString().Trim() + @"/";
                //myDevice = new Device();
                myDevice = DeviceEnumerator.GetDeviceFromResource(rNAME);
                string devName = myDevice.GetDeviceName();
                if (myDevice != null)
                {
                    myDevice.Reset();
                }
            }

            //string prNAME = cubeUri + "Dev" + "7" + @"/";
            //myDevice = DeviceEnumerator.GetDeviceFromResource(prNAME);
            //myDevice.Reset();

        }
        void DasReset2(string cubeuri)
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


        private System.Net.IPAddress CubeUriToIpAddress(string uri)
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
