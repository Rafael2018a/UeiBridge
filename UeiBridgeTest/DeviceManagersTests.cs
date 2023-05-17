using System;
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
            analogWriterMock writeMock = new analogWriterMock();
            //mk.OriginSession = sess1;

            BlockSensorSetup setup = new BlockSensorSetup(new EndPoint("192.168.19.2", 50455), "BlockSensor");
            setup.SlotNumber = BlockSensorSetup.BlockSensorSlotNumber;
            BlockSensorManager2 blocksensor = new BlockSensorManager2(setup, writeMock, sess1);
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
            //sess1.Dispose();
            blocksensor.Dispose();
            // 6 7 5

            Assert.Multiple(() =>
            {
                Assert.That(writeMock.Scan[0], Is.InRange(5.99, 6.01));
                Assert.That(writeMock.Scan[1], Is.InRange(6.99, 7.01));
                Assert.That(writeMock.Scan[2], Is.InRange(4.99, 5.01));
            });
        }


        //[SetUp]
        public void Setup1()
        {
            string url = "simu://";
            List<CubeSetup> csetupList = new List<CubeSetup>();
            List<UeiDeviceAdapter> devList = UeiBridge.Program.BuildDeviceList(new List<string> { url });
            var resList = devList.Select(d => new UeiDeviceAdapter( null, d.DeviceName, d.DeviceSlot));// as List<UeiDeviceAdapter>;
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
            var ao = devicelist.Where(i => i.DeviceName.StartsWith("Simu-AO16")).FirstOrDefault();
            AO308Setup setup = new AO308Setup(new EndPoint("8.8.8.8", 5000), new UeiDeviceAdapter(null, ao.DeviceName, ao.DeviceSlot));

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
            var ao = devicelist.Where(i => i.DeviceName.StartsWith("Simu-DIO64")).FirstOrDefault();

            DIO403Setup setup = new DIO403Setup(new EndPoint("8.8.8.8", 5000), null, new UeiDeviceAdapter(null, ao.DeviceName, ao.DeviceSlot));

            DIO403OutputDeviceManager dio403 = new DIO403OutputDeviceManager(setup, mk1, s);
            dio403.OpenDevice();

            var m = EthernetMessage.CreateMessage(4, 2, 0, new byte[] { 0xac, 0x13 });

            dio403.Enqueue(m.GetByteArray(MessageWay.downstream));

            System.Threading.Thread.Sleep(100);

            dio403.Dispose();

            Assert.Multiple(() =>
            {
                Assert.That(mk1.Scan[0], Is.EqualTo(0xac));
                Assert.That(mk1.Scan[1], Is.EqualTo(0x13));
            });
        }

        [Test] 
        public void DIO403InputDeviceManagerTest()
        {
            //Session s = new Session();
            //s.CreateDOChannel("simu://Dev2/Di3:5");
            //digitalWriterMock mk1 = new digitalWriterMock();
            ////mk1.OriginSession = s;
            //var devicelist = UeiBridge.Program.BuildDeviceList(new List<string>() { "simu://" });
            //var ao = devicelist.Where(i => i.DeviceName.StartsWith("Simu-DIO64")).FirstOrDefault();

            UeiDeviceAdapter uda = new UeiDeviceAdapter(null, DeviceMap2.DIO403Literal, 2); // simu://Simu-DIO64 is on slot 2

            DIO403Setup setup = new DIO403Setup(null, new EndPoint("8.8.8.8", 5000), uda);
            setup.CubeUrl = "simu://";

            SenderMock sm = new SenderMock();

            DIO403InputDeviceManager dio403 = new DIO403InputDeviceManager(sm, setup);
            dio403.OpenDevice();

            System.Threading.Thread.Sleep(100);

            dio403.Dispose();

            var so = sm._sentObject;
        }

        [Test]
        public void Sl508PreTest()
        {
            string resourceText = "simu://Dev4/Com0";
            Session SrlSession = new Session();
            SrlSession.CreateSerialPort(resourceText,
                                        SerialPortMode.RS232,
                                        SerialPortSpeed.BitsPerSecond14400,
                                        SerialPortDataBits.DataBits7,
                                        SerialPortParity.None,
                                        SerialPortStopBits.StopBits1,
                                        "");

        }
#if dont
        [Test]
        public void SL508inputTest()
        {
            // create session
            UeiDeviceAdapter devAd = new UeiDeviceAdapter("Simu-COM", 4);

            SL508892Setup setup = new SL508892Setup(null, null, devAd);
            setup.CubeUrl = "simu://";
            SerialSessionEx serialSession = new SerialSessionEx(setup);

            // emit info log
            foreach (Channel c in serialSession.GetChannels())
            {
                SerialPort sp1 = c as SerialPort;
                string s1 = sp1.GetSpeed().ToString();
                string s2 = s1.Replace("BitsPerSecond", "");
            }

            // build input manager
            //string instanceName = $"{realDevice.DeviceName}/Slot{realDevice.DeviceSlot}";
            ISend<SendObject> uWriter = null;
                //new UdpWriter(instanceName, setup.DestEndPoint.ToIpEp(), _mainConfig.AppSetup.SelectedNicForMCast);

            // build serial readers
            List<IReaderAdapter<byte[]>> serialReaderList = new List<IReaderAdapter<byte[]>>();
            for (int ch = 0; ch < serialSession.GetNumberOfChannels(); ch++)
            {

                var sr = new SerialReader(serialSession.GetDataStream(), serialSession.GetChannel(ch).GetIndex());
                var sr1 = new SerialReaderAdapter(sr);
                serialReaderList.Add(sr1);
            }

            // create input manager
            SL508InputDeviceManager indev = new SL508InputDeviceManager( serialReaderList, setup, uWriter);


        }
#endif
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
            
        }

        public void WriteSingleScan(UInt16[] scan)
        {
            Scan = scan;
        }
    }

    public class SenderMock : ISend<SendObject>
    {
        public SendObject _sentObject;
        public void Send(SendObject i)
        {
            _sentObject = i;
        }
    }

}
