using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using log4net;
using UeiDaq;

namespace UeiBridge
{
    class Program
    {
        //ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);
        ILog _logger = log4net.LogManager.GetLogger("Root");

        static void Main(string[] args)
        {
            Program p = new Program();
            p.Run();
            Console.ReadKey();
        }

        private void Run()
        {
            _logger.Error("logger check. ERROR");
            // prepare & display device list
            List<Device> deviceList = StaticMethods.GetDeviceList();
            if (null == deviceList)
            {
                _logger.Error(StaticMethods.LastErrorMessage);
                Console.ReadKey();
            }
            if (0 == deviceList.Count)
            {
                _logger.Warn("No device connected");
                Console.ReadKey();
                return;
            }

            _logger.Info(" *** Device list:");
            deviceList.ForEach(dev => _logger.Info($"{dev.GetDeviceName()} as Dev{dev.GetIndex()}"));
            _logger.Info(" *** End device list:");

            // prepare device dictionaries
            ProjectRegistry.Instance.Establish();
            // create instance for each output-device-manager
            ProjectRegistry.Instance.DeviceManagersDic.ToList().ForEach((pair) => pair.Value.Start());

            // init downwards objects
            EthernetToDevice e2d = new EthernetToDevice();
            e2d.Start();
            UdpReader ur = new UdpReader(e2d);
            ur.Start();

            // init upwards objects
            UdpWriter uw = new UdpWriter();
            DeviceToEthernet d2e = new DeviceToEthernet(uw);
            d2e.Start();
            DIO403InputDeviceManager dio403 = new DIO403InputDeviceManager( d2e, new TimeSpan(0,0,0,10, 1000), Config.Instance.DeviceUrl);
            dio403.Start();
            AI201InputDeviceManager ai200 = new AI201InputDeviceManager(d2e, new TimeSpan(0, 0, 0, 10, 1000), Config.Instance.DeviceUrl);
            ai200.Start();

            StartDownwardsTest();

            Console.ReadKey();
            
        }

        private void StartDownwardsTest()
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    UdpClient udpClient = new UdpClient();
                    System.Threading.Thread.Sleep(100);
                    IPEndPoint destEp = new IPEndPoint(IPAddress.Parse(Config.Instance.ReceiverMulticastAddress), Config.Instance.LocalPort);

                    for (int i=0; i<3; i++)
                    {

                        byte[] e403 = Make_DIO403Down_Message();
                        udpClient.Send(e403, e403.Length, destEp);

                        byte[] e308 = Make_A308Down_message();
                        udpClient.Send(e308, e308.Length, destEp); // ("192.168.201.202"),

                        byte[] e430 = Make_DIO430Down_Message();
                        udpClient.Send(e430, e308.Length, destEp);
                        

                        System.Threading.Thread.Sleep(5000);
                        
                    }
                }
                catch( Exception ex)
                {
                    _logger.Warn(ex.Message);
                }
            });
        }

        private byte[] Make_AO308_Message_old()
        {
            byte[] eth = new byte[32];
            Array.Clear(eth, 0, eth.Length);

            eth[0] = 0xAA;
            eth[1] = 0x55;

            Int16 val = 0;
            for (int i = 0; i < 8; i++)
            {
                byte[] bytes = BitConverter.GetBytes(val);
                bytes.CopyTo(eth, 16 + i * 2);
                val += 2000;
            }
            Int16 len = 32;
            byte[] bytes1 = BitConverter.GetBytes(len);
            bytes1.CopyTo(eth, 12);

            return eth;
        }

        byte [] Make_A308Down_message()
        {
            EthernetMessage msg = EthernetMessageFactory.CreateEmpty(0, 16);
            return msg.ToByteArrayDown();
        }
        private byte[] Make_DIO403Down_Message()
        {
            EthernetMessage msg = EthernetMessageFactory.CreateEmpty(4, 3);
            msg.PayloadBytes[0] = 0x12;
            msg.PayloadBytes[1] = 0x34;
            msg.PayloadBytes[2] = 0x56;

           return msg.ToByteArrayDown();
        }
        private byte[] Make_DIO430Down_Message()
        {
            EthernetMessage msg = EthernetMessageFactory.CreateEmpty(6, 16);
            return msg.ToByteArrayDown();
        }

        internal List<Device> ReadDeviceInfo()
        {
            DeviceCollection devColl = new DeviceCollection( Config.Instance.DeviceUrl);
            //int unknownDeviceIndex = 100;
            List<Device> resultList = new List<Device>();
            try
            {
                foreach (Device dev in devColl)
                {
                    
                    if (dev != null)
                    {
                        resultList.Add(dev);
                        //string name = dev.GetDeviceName();
                        //int index = dev.GetIndex();
                        //var res = dev.GetResourceName();
                        ////var y = dev.GetSerialNumber();
                        //var slot = dev.GetSlot();
                        //var m = dev.GetStatus();


                        //_logger.Info(string.Format($"{name} \tslot {slot} \t url:{res}"));
                        //int icd_idx = Find_Icd_DeviceIndex(name);
                        //if (icd_idx < 0)
                        //{
                        //    icd_idx = unknownDeviceIndex++;
                        //}


                        //{
                        //    //IDevice idev = FindCreateDeviceObject(name);
                        //    Type dt = GetDeviceType(name);
                        //    //if (dt != null)
                        //    {
                        //        DeviceMap.Add(icd_idx, new DeviceInfo(name, index, res, dt));
                        //    }
                        //}

                    }
                }
            }
            catch(Exception ex)
            {
                _logger.Error(ex.Message);
            }
            return resultList;
        }

    }
}
