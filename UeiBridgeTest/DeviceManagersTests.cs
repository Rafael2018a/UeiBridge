﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using UeiBridge;
using UeiBridge.Library;
using UeiBridge.Types;
using UeiDaq;

namespace UeiBridgeTest
{
    [TestFixture]
    class DeviceManagersTests
    {
        [Test]
        public void BlockSensorTest()
        {

            Session sess1 = new Session();
            sess1.CreateAOChannel("simu://Dev1/AO0:7", -10, +10);
            analogWriterMock mk = new analogWriterMock();
            //mk.OriginSession = sess1;

            BlockSensorSetup setup = new BlockSensorSetup(new EndPoint("192.168.19.2", 50455), "BlockSensor");
            BlockSensorManager blocksensor = new BlockSensorManager(setup, mk, sess1);
            blocksensor.OpenDevice();
            byte[] d403 = UeiBridge.Library.StaticMethods.Make_Dio403_upstream_message(new byte[] { 0x5, 0, 0 });
            blocksensor.Enqueue(d403);
            //double factor = AO308Setup.PeekVoltage_downstream / (Int16.MaxValue+1);
            Int16[] payload = new short[14];
            Array.Clear(payload, 0, payload.Length);
            payload[2] = AnalogConverter.PlusMinusVoltageToInt16(10.0, 5.0);
            payload[9] = AnalogConverter.PlusMinusVoltageToInt16(10.0, 6.0);
            payload[10] = AnalogConverter.PlusMinusVoltageToInt16(10.0, 7.0);
            EthernetMessage em = UeiBridge.Library.StaticMethods.Make_BlockSensor_downstream_message(payload);
            blocksensor.Enqueue(em.GetByteArray(MessageWay.downstream));
            //s.Stop();

            System.Threading.Thread.Sleep(1000);
            sess1.Dispose();
            // 6 7 5

            Assert.Multiple(() =>
            {
                Assert.That(mk.Scan[0], Is.InRange(5.99, 6.01));
                Assert.That(mk.Scan[1], Is.InRange(6.99, 7.01));
                Assert.That(mk.Scan[2], Is.InRange(4.99, 5.01));
            });
        }


        //[SetUp]
        public void Setup()
        {
            string url = "simu://";
            List<CubeSetup> csetupList = new List<CubeSetup>();
            List<DeviceEx> devList = UeiBridge.Program.BuildDeviceList(url);
            var resList = devList.Select(d => new UeiDeviceAdapter(d.PhDevice.GetDeviceName(), d.PhDevice.GetIndex()));// as List<UeiDeviceAdapter>;
            List<UeiDeviceAdapter> l = new List<UeiDeviceAdapter>(resList);
            csetupList.Add(new CubeSetup(l, url));

            // save default config to file
            //Config2.Instance = new Config2(csetupList);
        }

        [Test]
        public void AO308DeviceManagerTest()
        {

            Session session1 = new Session();
            session1.CreateAOChannel("simu://Dev1/AO0:7", -10, +10);
            analogWriterMock mk = new analogWriterMock();
            //mk.OriginSession = session1;
            var devicelist = UeiBridge.Program.BuildDeviceList(new List<string>() { "simu://" });
            var ao = devicelist.Where(i => i.PhDevice.GetDeviceName().StartsWith("Simu-AO16")).FirstOrDefault();
            AO308Setup setup = new AO308Setup(new EndPoint("8.8.8.8", 5000), new UeiDeviceAdapter(ao.PhDevice));

            AO308OutputDeviceManager ao308 = new AO308OutputDeviceManager(setup, mk, session1, false);
            
            ao308.OpenDevice();
            AnalogConverter ac = new AnalogConverter(10, 12);
            var d1 = new double[] { 5, 7, 9 };
            Byte[] bytes = ac.UpstreamConvert(d1);
            var m = EthernetMessage.CreateMessage(0, 1, 0, bytes);
            ao308.Enqueue(m.GetByteArray(MessageWay.downstream));
            System.Threading.Thread.Sleep(100);
            //s.Stop();
            //session1.Dispose();
            ao308.Dispose();

            Assert.Multiple(() =>
            {
                for (int i = 0; i < mk.Scan.Length; i++)
                {
                    Assert.That(mk.Scan[i] - d1[i], Is.InRange(-0.1, 0.1));
                }
            });
        }

        [Test]
        public void DIO403OutDeviceMangerTest()
        {
            Session s = new Session();
            s.CreateDOChannel("simu://Dev2/Do0:2");
            digitalWriterMock mk1 = new digitalWriterMock();
            //mk1.OriginSession = s;
            var devicelist = UeiBridge.Program.BuildDeviceList(new List<string>() { "simu://" });
            var ao = devicelist.Where(i => i.PhDevice.GetDeviceName().StartsWith("Simu-DIO64")).FirstOrDefault();

            DIO403Setup setup = new DIO403Setup(new EndPoint("8.8.8.8", 5000), null, new UeiDeviceAdapter(ao.PhDevice));

            DIO403OutputDeviceManager dio403 = new DIO403OutputDeviceManager(setup, mk1, s);
            dio403.OpenDevice();

            var m = EthernetMessage.CreateMessage(4, 2, 0, new byte[] { 0xac, 0x13 });

            dio403.Enqueue(m.GetByteArray(MessageWay.downstream));

            System.Threading.Thread.Sleep(100);

            Assert.Multiple(() =>
            {
                Assert.That(mk1.Scan[0], Is.EqualTo(0xac));
                Assert.That(mk1.Scan[1], Is.EqualTo(0x13));
            });

        }
    }
    public class analogWriterMock : IWriterAdapter<double[]>
    {
        //public int NumberOfChannels => 8;

        //int IAnalogWrite.NumberOfChannels { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public double[] Scan { get; set; }

        //public Session OriginSession { get; set; }

        public void Dispose()
        {
            
        }

        public void WriteSingleScan(double[] scan)
        {
            Scan = scan;
        }
    }
    public class digitalWriterMock : IWriterAdapter<UInt16[]>
    {
        //public int NumberOfChannels => 8;

        //int IAnalogWrite.NumberOfChannels { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public UInt16[] Scan { get; set; }

        //public Session OriginSession { get; set; }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void WriteSingleScan(UInt16[] scan)
        {
            Scan = scan;
        }
    }

}
