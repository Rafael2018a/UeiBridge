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
            BlockSensorManager2 blocksensor = new BlockSensorManager2(setup, new SessionAdapter(sess1));
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

        [Test]
        public void AO308DeviceManagerTest()
        {
            string simuUrl = "simu://";
            Session session1 = new Session();
            session1.CreateAOChannel("simu://Dev1/AO0:7", -10, +10);
            session1.ConfigureTimingForSimpleIO();
            //analogWriterMock mk = new analogWriterMock();
            //mk.OriginSession = session1;
            //var devicelist = UeiBridge.Program.BuildLinearDeviceList(new List<string>() { simuUrl });
            //var ao = devicelist.Where(i => i.DeviceName.StartsWith("Simu-AO16")).FirstOrDefault();
            AO308Setup setup = new AO308Setup(new EndPoint("8.8.8.8", 5000), new UeiDeviceInfo(simuUrl, 1, DeviceMap2.AO308Literal));
            setup.CubeUrl = simuUrl;
            SessionAdapter sa = new SessionAdapter(session1);
            //SessionMock sm = new SessionMock(4);
            AO308OutputDeviceManager ao308 = new AO308OutputDeviceManager(setup, sa, false);
            ao308.OpenDevice();
            AnalogConverter ac = new AnalogConverter(10, 12);
            var d1 = new double[] { 5, 7, 9 };
            Byte[] bytes = ac.UpstreamConvert(d1);
            var m = EthernetMessage.CreateMessage(0, 1, 0, bytes);
            ao308.Enqueue(m.GetByteArray(MessageWay.downstream));
            System.Threading.Thread.Sleep(100);

            //var s = sa.GetAnalogScaledReader();
            //s.Stop();
            //session1.Dispose();
            ao308.Dispose();

            Assert.Multiple(() =>
            {
                //for (int i = 0; i < mk.Scan.Length; i++)
                //{
                //    Assert.That(mk.Scan[i] - d1[i], Is.InRange(-0.1, 0.1));
                //}
            });
        }

        [Test]
        public void DigitalSimuDIO64_OutDeviceMangerTest()
        {
            Session s = new Session();
            s.CreateDOChannel("simu://Dev2/Do0,1"); // 4 channels, no more (empiric).
            s.ConfigureTimingForSimpleIO();
            int noc = s.GetNumberOfChannels();


            DIO403Setup setup = new DIO403Setup(new EndPoint("8.8.8.8", 5000), null, new UeiDeviceInfo("simu://", 2, "Simu-DIO64"), noc);

            DIO403OutputDeviceManager dio403 = new DIO403OutputDeviceManager(setup, new SessionAdapter(s));
            dio403.OpenDevice();

            var m = EthernetMessage.CreateMessage(4, 2, 0, new byte[] { 0xac, 0x13, 0x21, 0x22 });

            dio403.Enqueue(m.GetByteArray(MessageWay.downstream));

            System.Threading.Thread.Sleep(100);

            dio403.Dispose();

            Assert.Multiple(() =>
            {
                //Assert.That(ok, Is.EqualTo(true));
                //Assert.That(mk1.Scan[0], Is.EqualTo(0xac));
                //Assert.That(mk1.Scan[1], Is.EqualTo(0x13));
            });
        }

        [Test]
        public void DIO403InputDeviceManagerTest() // 
        {
            //Session s = new Session();
            string cubeurl = "pdna://192.168.100.2";//c";

            UeiDaq.DeviceCollection devColl = new UeiDaq.DeviceCollection(cubeurl);
            List<UeiDeviceInfo> devList1 = UeiBridge.Library.StaticMethods.DeviceCollectionToDeviceInfoList(devColl, cubeurl);

            if (devList1.Count > 1) // if connected
            {

                UeiDeviceInfo info = new UeiDeviceInfo(cubeurl, 2, DeviceMap2.DIO403Literal); // simu://Simu-DIO64 is on slot 2

                Session inSession = new Session();
                inSession.CreateDIChannel(cubeurl + "/Dev5/Di3:5");
                inSession.ConfigureTimingForSimpleIO();
                SessionAdapter sa = new SessionAdapter(inSession);

                DIO403Setup setup = new DIO403Setup(null, new EndPoint("8.8.8.8", 5000), info, 6);
                setup.CubeUrl = cubeurl;

                SenderMock sm = new SenderMock();

                DIO403InputDeviceManager dio403 = new DIO403InputDeviceManager(setup, sa, sm);
                bool ok = dio403.OpenDevice();



                System.Threading.Thread.Sleep(100);

                dio403.Dispose();



                string errStr = null;
                EthernetMessage em = EthernetMessage.CreateFromByteArray(sm._sentObject.ByteMessage, MessageWay.upstream, ref errStr);

                Assert.Multiple(() =>
                {
                    Assert.That(ok, Is.EqualTo(true));
                    Assert.That(sm._sentObject.ByteMessage[0], Is.EqualTo(0x55));
                    Assert.That(em.NominalLength, Is.EqualTo(22));
                });
            }
        }
        [Test]
        public void DIO403InputDeviceManagerTest2() // this test does not need connected cube
        {
            string cubeurl = "pdna://192.168.100.2";
            UeiDeviceInfo info = new UeiDeviceInfo(cubeurl, 2, DeviceMap2.DIO403Literal);

            ISession sa = new SessionMock(6);

            DIO403Setup setup = new DIO403Setup(null, new EndPoint("8.8.8.8", 5000), info, 6);
            setup.CubeUrl = cubeurl;

            SenderMock sm = new SenderMock();

            DIO403InputDeviceManager dio403 = new DIO403InputDeviceManager(setup, sa, sm);
            bool ok = dio403.OpenDevice();

            System.Threading.Thread.Sleep(100);

            dio403.Dispose();

            Assert.Multiple(() =>
            {
                Assert.That(ok, Is.EqualTo(true));
                Assert.That(sm._sentObject.ByteMessage[0], Is.EqualTo(0x55));
                Assert.That(sm._sentObject.ByteMessage.Length, Is.EqualTo(22));
            });
        }

        [Test]
        public void DIO403OutputDeviceTest()
        {
            string cubeurl = "pdna://192.168.100.2";
            UeiDeviceInfo info = new UeiDeviceInfo(cubeurl, 2, DeviceMap2.DIO403Literal);

            DIO403Setup setup = new DIO403Setup(new EndPoint("8.8.8.8", 5000), null, info, 6);
            setup.CubeUrl = cubeurl;
            SessionMock sm = new SessionMock(6);
            DIO403OutputDeviceManager dio403 = new DIO403OutputDeviceManager(setup, sm);
            dio403.OpenDevice();
            EthernetMessage em = EthernetMessage.CreateMessage(DeviceMap2.GetCardIdFromCardName(DeviceMap2.DIO403Literal), 2, 2, new byte[] { 1, 2, 3, 4, 5, 6 });
            dio403.Enqueue(em.GetByteArray(MessageWay.downstream));
            System.Threading.Thread.Sleep(200);
            dio403.Dispose();

            Assert.Multiple(() =>
            {
                //Assert.That(sm._DigitalWriterMock.Scan, Is.Not.Null);
                //Assert.That(sm._DigitalWriterMock.Scan.Count, Is.EqualTo(6));
                //Assert.That(sm._DigitalWriterMock.Scan[5], Is.EqualTo(6));
            });
        }

        [Test]
        public void SerialSessionTest()
        {
            UeiDeviceInfo di = new UeiDeviceInfo("simu://", 4, DeviceMap2.SL508Literal);

            SL508892Setup deviceSetup = new SL508892Setup(null, null, di);

            deviceSetup.Channels[1].Baudrate = SerialPortSpeed.BitsPerSecond14400;
            deviceSetup.CubeUrl = "simu://";
            SessionEx sx = new SessionEx(deviceSetup);
            SerialPort ch = sx.GetChannel(1) as SerialPort;
            var speed1 = ch.GetSpeed();
            ch = sx.GetChannel(0) as SerialPort;
            var speed0 = ch.GetSpeed();

            Assert.That(speed0, Is.EqualTo(SerialPortSpeed.BitsPerSecond19200));
            Assert.That(speed1, Is.EqualTo(SerialPortSpeed.BitsPerSecond14400));
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
    public class DigitalWriterMock : IWriterAdapter<UInt16[]>
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

    public class digitalReaderMock : IReaderAdapter<UInt16[]>
    {
        int _numberOfChennels;

        public digitalReaderMock(int numberOfChennels)
        {
            _numberOfChennels = numberOfChennels;
        }

        public void Dispose()
        {
            
        }

        public UInt16[] ReadSingleScan()
        {
            UInt16[] result = new ushort[_numberOfChennels];
            result[0] = 0xAC13;
            return result;
        }
    }
    public class SenderMock : ISend<SendObject>
    {
        public SendObject _sentObject;

        public void Dispose()
        {

        }

        public void Send(SendObject i)
        {
            _sentObject = i;
        }

    }

    public class SessionMock : ISession
    {
        public int _numberOfChennels;
        public List<IChannel> _channelList = new List<IChannel>();
        public DigitalWriterMock _digitalWriterMock;
        public digitalReaderMock _digitalReader;
        public analogWriterMock _analogWriter;
        public DeviceMock _deviceMock;

        public SessionMock(int numberOfChannels)
        {
            for (int i = 0; i < numberOfChannels; i++)
            {
                _channelList.Add(new ChannelMock(i));
            }
            _numberOfChennels = numberOfChannels;
        }

        public void Dispose()
        {

        }

        public IChannel GetChannel(int v)
        {
            return _channelList[v];
        }

        public List<IChannel> GetChannels()
        {
            return _channelList;
        }

        public IWriterAdapter<ushort[]> GetDigitalWriter()
        {
            if (null == _digitalWriterMock)
            {
                _digitalWriterMock = new DigitalWriterMock();
            }
            return _digitalWriterMock;
        }

        public int GetNumberOfChannels()
        {
            return _channelList.Count;
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }

        IReaderAdapter<ushort[]> ISession.GetDigitalReader()
        {
            if (null == _digitalReader)
            {
                _digitalReader = new digitalReaderMock(_numberOfChennels);
            }
            return _digitalReader;
        }

        public IWriterAdapter<double[]> GetAnalogScaledReader()
        {
            if (null == _analogWriter)
            {
                _analogWriter = new analogWriterMock();
            }
            return _analogWriter;
        }

        IDevice ISession.GetDevice()
        {
            if (null==_deviceMock)
            {
                _deviceMock = new DeviceMock();
            }
            return _deviceMock;
        }

        public IWriterAdapter<double[]> GetAnalogScaledWriter()
        {
            throw new NotImplementedException();
        }
    }
    public class DeviceMock : IDevice
    {
        public Range[] GetAORanges()
        {
            Range r = new Range();
            r.maximum = 10.0;
            r.minimum = 10.0;
            return new Range[] { r };
        }
    }
    public class DigitalReaderMock : IReaderAdapter<UInt16[]>
    {
        int _numOfChannels;

        public DigitalReaderMock(int numOfChannels)
        {
            _numOfChannels = numOfChannels;
        }

        public void Dispose()
        {

        }

        ushort[] IReaderAdapter<ushort[]>.ReadSingleScan()
        {
            ushort[] scan = new ushort[_numOfChannels];
            for (int c = 0; c < _numOfChannels; c++)
            {
                scan[c] = 0x1122;
            }
            return scan;
        }
    }
    public class AnalogWriterMock : IWriterAdapter<double[]>
    {
        public double[] Scan { get; private set; }
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void WriteSingleScan(double[] scan)
        {
            Scan = scan;
        }
    }
    public class DigitalWriterMock2 : IWriterAdapter<UInt16>
    {
        public UInt16 LastScan { set; get; }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void WriteSingleScan(ushort scan)
        {
            LastScan = scan;
        }
    }
    public class ChannelMock : IChannel
    {
        int _channelIndex;

        public ChannelMock(int channelIndex)
        {
            _channelIndex = channelIndex;
        }

        public int GetIndex()
        {
            return _channelIndex;
        }

        public string GetResourceName()
        {
            return "simu://Dev2/Do0:3";
        }
    }

}
