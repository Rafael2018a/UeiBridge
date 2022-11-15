﻿using System;
using System.Collections;
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
        ILog _logger = StaticMethods.GetLogger();
        List<InputDevice> _inputDevices = new List<InputDevice>();

        static void Main(string[] args)
        {
            Program p = new Program();
            p.Run();
            System.Threading.Thread.Sleep(1000);
        }

        private void Run()
        {
            
            // print current version
            var v = System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString();
            _logger.Info($"UEI Bridge. Version {v} Device URL: {Config.Instance.DeviceUrl}");

            // prepare device list
            List<Device> deviceList = StaticMethods.GetDeviceList();
            if (null == deviceList)
            {
                _logger.Error(StaticMethods.LastErrorMessage);
                return;
            }
            if (0 == deviceList.Count)
            {
                _logger.Warn("No device connected");
                Console.ReadKey();
                return;
            }

            // display device list
            _logger.Info(" *** Device list:");
            //deviceList.ForEach(dev => _logger.Info($"{dev.GetDeviceName()} as Dev{dev.GetIndex()}"));
            foreach(var dev in deviceList)
            {
                _logger.Info($"{dev.GetDeviceName()} as Dev{dev.GetIndex()}");
            }
            _logger.Info(" *** End device list:");

            // prepare device dictionaries
            ProjectRegistry.Instance.Establish();

            // init downwards objects
            EthernetToDevice e2d = new EthernetToDevice();
            e2d.Start();
            UdpReader ur = new UdpReader(e2d, "from eth");
            ur.Start();

            // init upwards objects
            UdpWriter uw = new UdpWriter(Config.Instance.SenderMulticastAddress, Config.Instance.SenderMulticastPort, "to eth", Config.Instance.LocalBindNicAddress);
            DeviceToEthernet d2e = new DeviceToEthernet(uw);
            d2e.Start();

            // start input device managers
            // ============================
            _inputDevices.Add(new DIO403InputDeviceManager(d2e, new TimeSpan(0, 0, 0, 0, 10), Config.Instance.DeviceUrl));
            _inputDevices.Add(new AI201InputDeviceManager(d2e, new TimeSpan(0, 0, 0, 0, 10), Config.Instance.DeviceUrl));
            ProjectRegistry.Instance.SerialInputDeviceManager = new SL508InputDeviceManager(d2e, new TimeSpan(0, 0, 0, 0, 10), Config.Instance.DeviceUrl);
            _inputDevices.Add( ProjectRegistry.Instance.SerialInputDeviceManager);
            _inputDevices.ForEach(dev => dev.Start());

            // start output-device managers
            ProjectRegistry.Instance.OutputDevicesMap.ToList().ForEach((pair) => pair.Value.Start());

            // verify attached convertes
            if (false == ProjectRegistry.Instance.OutputDevicesMap.All(item => item.Value.AttachedConverter != null))
            {
                _logger.Warn("One of output device managers does not have attached converter");
            }
            if (false == _inputDevices.All(item => item.AttachedConverter != null))
            {
                _logger.Warn("One of input device managers does not have attached converter");
            }

            // self tests
            StartDownwardsTest();

            // publish status to StatusViewer
            Task.Factory.StartNew(() => PublishStatus_Task());

            Console.ReadKey();
            _logger.Info("Disposing....");

            // Dispose
            _inputDevices.ForEach(dev => dev.Dispose());
            ProjectRegistry.Instance.OutputDevicesMap.ToList().ForEach( dev => dev.Value.Dispose());
            
        }

        void PublishStatus_Task()
        {
            UdpWriter uw = new UdpWriter("239.10.10.17", 5093, "to statusViewer", Config.Instance.LocalBindNicAddress);

            while (true)
            {
                foreach (var item in ProjectRegistry.Instance.OutputDevicesMap)
                {
                    UeiLibrary.JsonStatusClass js = new UeiLibrary.JsonStatusClass(item.Value.DeviceName + " (Output)", item.Value.GetFormattedStatus() ); 
                    string s = Newtonsoft.Json.JsonConvert.SerializeObject(js);
                    byte[] send_buffer = Encoding.ASCII.GetBytes(s);
                    uw.Send(send_buffer);
                }

                foreach (var item in _inputDevices)
                {
                    UeiLibrary.JsonStatusClass js = new UeiLibrary.JsonStatusClass(item.DeviceName + " (Input)", item.GetFormattedStatus());
                    string s = Newtonsoft.Json.JsonConvert.SerializeObject(js);
                    byte[] send_buffer = Encoding.ASCII.GetBytes(s);
                    uw.Send(send_buffer);
                }

                System.Threading.Thread.Sleep(1000);
            }
        }

        private void StartDownwardsTest()
        {
            Task.Factory.StartNew(() =>
            {
                _logger.Info("Downward message simultion active.");

                try
                {
                    UdpClient udpClient = new UdpClient();
                    System.Threading.Thread.Sleep(100);
                    IPEndPoint destEp = new IPEndPoint(IPAddress.Parse(Config.Instance.ReceiverMulticastAddress), Config.Instance.ReceiverMulticastPort);

                    for (int i=0; i<10000; i++)
                    {

                        // digital out
                        byte[] e403 = StaticMethods.Make_DIO403Down_Message();
                        udpClient.Send(e403, e403.Length, destEp);

                        // analog out
                        byte[] e308 = StaticMethods.Make_A308Down_message();
                        udpClient.Send(e308, e308.Length, destEp);
#if notready
                        byte[] e430 = StaticMethods.Make_DIO430Down_Message();
                        udpClient.Send(e430, e308.Length, destEp);
#endif
                        // serial out
                        List<byte[]> e508 = StaticMethods.Make_SL508Down_Messages( i);
                        foreach (byte [] msg in e508)
                        {
                            udpClient.Send(msg, msg.Length, destEp);
                            System.Threading.Thread.Sleep(200);
                        }

                        System.Threading.Thread.Sleep(10);
                    }
                    _logger.Info("Downward message simulation end.");

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

        

    }
}
