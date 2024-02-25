using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using UeiBridge;
using UeiBridge.Library.CubeSetupTypes;
using UeiBridge.Library.Interfaces;
using UeiBridge.Library;
using UeiBridge.Library.Types;
using UeiDaq;

namespace UeiBridgeTest
{
    [TestFixture]
    [DefaultFloatingPointTolerance(1)]
    class DeviceManagersTests
    {
        [Test]
        public void SerialDeviceTest()
        {
            Session serialSession = null;

            {
                serialSession = new Session();

                {
                    string finalUrl = $"simu://Dev4/Com0";
                    SerialPort sport = serialSession.CreateSerialPort(finalUrl,
                                        SerialPortMode.RS232,
                                        SerialPortSpeed.BitsPerSecond14400,
                                        SerialPortDataBits.DataBits8,
                                        SerialPortParity.None,
                                        SerialPortStopBits.StopBits1,
                                        "");
                }
            }

            serialSession.Dispose();
        }

        
        public void BlockSensorTest()
        {
            Session sess1 = new Session();
            sess1.CreateAOChannel("simu://Dev1/AO0:7", -10, +10);
            sess1.ConfigureTimingForSimpleIO();
            sess1.Start();

            UeiDeviceInfo dinfo = new UeiDeviceInfo("", 0, "BlockSensor");
            BlockSensorSetup setup = new BlockSensorSetup(new EndPoint("192.168.19.2", 50455), dinfo);
            //setup.SlotNumber = BlockSensorSetup.devBlockSensorSlotNumber
            SessionAdapter sa = new SessionAdapter(sess1);
            BlockSensorManager2 blocksensor = new BlockSensorManager2(setup, sa);
            blocksensor.OpenDevice();
            byte[] d403 = UeiBridge.Library.StaticMethods.Make_Dio403_upstream_message(new byte[] { 0x5, 0, 0, 0, 0, 0 });
            blocksensor.Enqueue(d403);

            Int16[] payload = new short[14];
            Array.Clear(payload, 0, payload.Length);
            payload[2] = AnalogConverter.PlusMinusVoltageToInt16(10.0, 5.0);
            payload[9] = AnalogConverter.PlusMinusVoltageToInt16(10.0, 6.0);
            payload[10] = AnalogConverter.PlusMinusVoltageToInt16(10.0, 7.0);
            EthernetMessage em = UeiBridge.Library.StaticMethods.Make_BlockSensor_downstream_message(payload);
            blocksensor.Enqueue(em.GetByteArray(MessageWay.downstream));

            System.Threading.Thread.Sleep(500);
            var ls = sa.GetAnalogScaledWriter().LastScan;
            Assert.That(ls[2], Is.EqualTo(5.0));
            System.Threading.Thread.Sleep(500);
            blocksensor.Dispose();
        }
        [Test]
        public void AO308DeviceManagerTest()
        {
            string simuUrl = "simu://";
            // build session
            Session session1 = new Session();
            session1.CreateAOChannel("simu://Dev1/AO0:7", -10, +10);
            session1.ConfigureTimingForSimpleIO();
            AO308Setup setup = new AO308Setup(new EndPoint("8.8.8.8", 5000), new UeiDeviceInfo(simuUrl, 1, DeviceMap2.AO308Literal))
            {
                CubeUrl = simuUrl
            };
            SessionAdapter sa = new SessionAdapter(session1);

            // build device manager
            AO308OutputDeviceManager ao308 = new AO308OutputDeviceManager(setup, sa, false);
            ao308.OpenDevice();

            // enq
            AnalogConverter ac = new AnalogConverter(10, 12);
            var d1 = new double[] { 5, 7, 9 };
            Byte[] bytes = ac.UpstreamConvert(d1);
            var m = EthernetMessage.CreateMessage(0, 1, 0, bytes);
            ao308.Enqueue(m.GetByteArray(MessageWay.downstream));

            // wait
            System.Threading.Thread.Sleep(100);

            ao308.Dispose();

            // check result
            Assert.Multiple(() =>
            {
                var ls = sa.GetAnalogScaledWriter().LastScan;
                for (int i = 0; i < ls.Length; i++)
                {
                    Assert.That(ls[i], Is.EqualTo(d1[i]));//, Is.InRange(-0.1, 0.1));
                }
            });
        }
        [Test]
        public void DIO403OutputDeviceManagerTest()
        {
            Session sess1 = new Session();
            sess1.CreateDOChannel("simu://Dev2/Do0:3"); // 4 channels, no more (empiric).
            sess1.ConfigureTimingForSimpleIO();
            SessionAdapter sa = new SessionAdapter(sess1);

            DIO403Setup setup = new DIO403Setup(new EndPoint("8.8.8.8", 5000), null, new UeiDeviceInfo("simu://", 2, "Simu-DIO64"), sess1.GetNumberOfChannels());
            setup.CubeUrl = "simu://";

            // build device manager
            DIO403OutputDeviceManager dio403 = new DIO403OutputDeviceManager(setup, sa);
            dio403.OpenDevice();

            // enq
            var v = new byte[] { 0xac, 0x13, 0x21, 0x22 };
            var m = EthernetMessage.CreateMessage(4, 2, 0, v);
            dio403.Enqueue(m.GetByteArray(MessageWay.downstream));

            System.Threading.Thread.Sleep(100);

            dio403.Dispose();

            Assert.Multiple(() =>
            {
                var ls = sa.GetDigitalWriter().LastScan;
                for (int i = 0; i < ls.Length; i++)
                {
                    Assert.That(ls[i], Is.EqualTo(v[i]));
                }
            });
        }

        [Test]
        public void DIO403InputDeviceManagerTest() // based on connected cube
        {
            //Session s = new Session();
            string cubeurl = "pdna://192.168.100.2";//c";

            UeiCube cube = new UeiCube(cubeurl);
            //UeiDaq.DeviceCollection devColl = new UeiDaq.DeviceCollection(cubeurl);
            List<UeiDeviceInfo> devList1 = cube.GetDeviceInfoList();// UeiBridge.Library.StaticMethods.DeviceCollectionToDeviceInfoList(devColl, cubeurl);

            if (devList1.Count > 1) // if connected
            {

                UeiDeviceInfo info = new UeiDeviceInfo(cubeurl, 2, DeviceMap2.DIO403Literal); // simu://Simu-DIO64 is on slot 2

                Session inSession = new Session();
                inSession.CreateDIChannel(cubeurl + "/Dev5/Di3:5");
                inSession.ConfigureTimingForSimpleIO();
                SessionAdapter sa = new SessionAdapter(inSession);

                DIO403Setup setup = new DIO403Setup(null, new EndPoint("8.8.8.8", 5000), info, 6)
                {
                    CubeUrl = cubeurl
                };

                SenderMock sm = new SenderMock();

                DIO403InputDeviceManager dio403 = new DIO403InputDeviceManager(setup, sa, sm);
                bool ok = dio403.OpenDevice();



                System.Threading.Thread.Sleep(100);

                EthernetMessage em = EthernetMessage.CreateFromByteArray(sm._sentObject.ByteMessage, MessageWay.upstream, null);

                Assert.Multiple(() =>
                {
                    Assert.That(ok, Is.EqualTo(true));
                    Assert.That(sm._sentObject.ByteMessage[0], Is.EqualTo(0x55));
                    Assert.That(em.TotalLength, Is.EqualTo(22));
                });

                dio403.Dispose();
            }
        }
        [Test]
        public void DIO403InputDeviceManagerTest2() // this test does not need connected cube
        {
            string cubeurl = "simu://Dev2/Di0:1";

            Session sess1 = new Session();
            sess1.CreateDIChannel(cubeurl); // 4 channels, no more (empiric).
            sess1.ConfigureTimingForSimpleIO();
            SessionAdapter sa = new SessionAdapter(sess1);

            DIO403Setup setup = new DIO403Setup(null, new EndPoint("8.8.8.8", 5000), new UeiDeviceInfo("simu://", 2, DeviceMap2.DIO403Literal), sess1.GetNumberOfChannels())
            {
                CubeUrl = cubeurl
            };

            SenderMock sm = new SenderMock();

            DIO403InputDeviceManager dio403 = new DIO403InputDeviceManager(setup, sa, sm);
            bool ok = dio403.OpenDevice();

            System.Threading.Thread.Sleep(100);

            var last = sa.GetDigitalReader().LastScan;

            //Assert.That(ls.Length, Is.EqualTo(sa.GetNumberOfChannels()));

            dio403.Dispose();
            //Assert.Multiple(() =>
            //{
            //    Assert.That(ok, Is.EqualTo(true));
            //    Assert.That(sm._sentObject.ByteMessage[0], Is.EqualTo(0x55));
            //    Assert.That(sm._sentObject.ByteMessage.Length, Is.EqualTo(22));
            //});
        }

        [Test]
        public void AnalogInTest()
        {
            Session ses1 = new Session();
            ses1.CreateAIChannel("simu://Dev0/Ai0", -10, +10, AIChannelInputMode.SingleEnded);
            ses1.ConfigureTimingForSimpleIO();
        }

        [Test]
        public void SerialSessionTest()
        {
            UeiDeviceInfo di = new UeiDeviceInfo("simu://", 4, DeviceMap2.SL508Literal);

            SL508892Setup thisSetup = new SL508892Setup(null, null, di, System.IO.FileAccess.ReadWrite);

            thisSetup.Channels[1].Baudrate = SerialPortSpeed.BitsPerSecond14400;
            thisSetup.CubeUrl = "simu://";
            Session serialSession = new Session();

            {
                foreach (var channel in thisSetup.Channels)
                {
                    string finalUrl = $"{thisSetup.CubeUrl}Dev{thisSetup.SlotNumber}/Com{channel.ComIndex}";
                    var port = serialSession.CreateSerialPort(finalUrl,
                                        channel.Mode,
                                        channel.Baudrate,
                                        SerialPortDataBits.DataBits8,
                                        channel.Parity,
                                        channel.Stopbits,
                                        "");
                }

                {
                    int chCount = thisSetup.Channels.Where(ch1 => ch1.IsEnabled == true).ToList().Count;
                    Assert.That(serialSession.GetNumberOfChannels(), Is.EqualTo(chCount));
                }

                serialSession.ConfigureTimingForMessagingIO(1000, 100.0);
                serialSession.GetTiming().SetTimeout(5000); // timeout to throw from _serialReader.EndRead (looks like default is 1000)

                //serialSession.ConfigureTimingForSimpleIO();

                serialSession.Start();

                //IsValidSession = true;
            }


            SerialPort ch = serialSession.GetChannel(1) as SerialPort;
            var speed1 = ch.GetSpeed();
            ch = serialSession.GetChannel(0) as SerialPort;
            var speed0 = ch.GetSpeed();

            Assert.That(speed0, Is.EqualTo(SerialPortSpeed.BitsPerSecond115200));
            Assert.That(speed1, Is.EqualTo(SerialPortSpeed.BitsPerSecond14400));

            serialSession.Stop();
            serialSession.Dispose();
        }

        [Test]
        public void SessionAdapterClassTest()
        {
            string cubeurl = "simu://Dev2/Di0:1";

            Session sess1 = new Session();
            sess1.CreateDIChannel(cubeurl); // 4 channels, no more (empiric).
            sess1.ConfigureTimingForSimpleIO();
            SessionAdapter sa = new SessionAdapter(sess1);

            DIO403Setup setup = new DIO403Setup(null, new EndPoint("8.8.8.8", 5000), new UeiDeviceInfo("simu://", 2, DeviceMap2.DIO403Literal), sess1.GetNumberOfChannels())
            {
                CubeUrl = cubeurl
            };

            SenderMock sm = new SenderMock();

            DIO403InputDeviceManager dio403 = new DIO403InputDeviceManager(setup, sa, sm);
            bool ok = dio403.OpenDevice();

            System.Threading.Thread.Sleep(100);

            var last = sa.GetDigitalReader().LastScan;

            Assert.That(last, Is.Not.Null);

            //dio403.Dispose();

        }

        [Test]
        public void GetDeviceFromResourceTest()
        {
            string deviceUri = "simu://dev0";
            Device dev = DeviceEnumerator.GetDeviceFromResource(deviceUri);
            Assert.That(dev, Is.Not.Null);
            deviceUri = "simu://dev14";
            dev = DeviceEnumerator.GetDeviceFromResource(deviceUri);
            Assert.That(dev, Is.Null);
            //deviceUri = "pdna://192.168.100.3/dev0";
            //dev = DeviceEnumerator.GetDeviceFromResource(deviceUri);
            //Assert.That(dev, Is.Not.Null); // this works only if cube is connected
            //deviceUri = "192.168.100.3/dev0";
            //dev = DeviceEnumerator.GetDeviceFromResource(deviceUri);
            //Assert.That(dev, Is.Null);
            //deviceUri = "simu://dev14";
            //dev = DeviceEnumerator.GetDeviceFromResource(deviceUri);
            //Assert.That(dev, Is.Null);
        }
        [Test]
        public void SerialWatchDogCrashTest()
        {
            Action<string, string> wdAction;
            Tuple<string, string> pair = new Tuple<string, string>("", "");
            wdAction = new Action<string, string>((orig, reason) =>
            {
                //Console.WriteLine($"WD Event: Originator{orig}; Reason{reason}");  
                pair = new Tuple<string, string>(orig, reason);
                //pair.Item1 = orig;
                //pair.Item2 = reason;
            });
            DeviceWatchdog wd = new DeviceWatchdog(wdAction);
            string origname = "testorig";
            string reasonname = "testreason";
            wd.Register(origname, TimeSpan.FromSeconds(1));
            wd.NotifyCrash(origname, reasonname);
            wd.Dispose();

            Assert.That(pair.Item1 == origname && pair.Item2 == reasonname);
        }
        [Test]
        public void SerialWatchDogKeepAliveTest()
        {
            Action<string, string> wdAction;
            Tuple<string, string> pair = new Tuple<string, string>("", "");
            wdAction = new Action<string, string>((orig, reason) =>
            {
                //Console.WriteLine($"WD Event: Originator{orig}; Reason{reason}");  
                pair = new Tuple<string, string>(orig, reason);
                //pair.Item1 = orig;
                //pair.Item2 = reason;
            });
            DeviceWatchdog wd = new DeviceWatchdog(wdAction);
            string origname = "testorig";
            wd.Register(origname, TimeSpan.FromSeconds(1));
            System.Threading.Thread.Sleep(1100);
            wd.Dispose();

            Assert.That(pair.Item1 == origname && pair.Item2 == "Not alive");
        }
        [Test]
        public void UdpWriterAsyncTest()
        {
            SenderMock sm = new SenderMock();
            UInt16 prmbl = BitConverter.ToUInt16(new byte[] { 0xac, 0x13 }, 0);
            UdpWriterAsync asyncWriter = new UdpWriterAsync(sm, prmbl);
            asyncWriter.Start();
            Func<byte[], byte[]> mbuilder = (m => m);
            for (int i = 0; i < 1; i++)
            {
                asyncWriter.Enqueue(new SendObject2(null, mbuilder, new byte[] { 0x1c, 0x13 }));
                asyncWriter.Enqueue(new SendObject2(null, mbuilder, new byte[] { 0xac, 0x13, 0, 0 }));
                asyncWriter.Enqueue(new SendObject2(null, mbuilder, new byte[] { 0xac, 0x13 }));
                asyncWriter.Enqueue(new SendObject2(null, mbuilder, new byte[] { 0xac, 0x13, 1, 2, 0xac, 0x13, 10, 11, 12 }));
            }
            System.Threading.Thread.Sleep(1000);
            asyncWriter.Dispose();
            var v = asyncWriter._lengthCountList;

        }
    }
    public class AnalogWriterMock : IWriterAdapter<double[]>
    {
        //public int NumberOfChannels => 8;

        //int IAnalogWrite.NumberOfChannels { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public double[] Scan { get; set; }

        public double[] LastScan => throw new NotImplementedException();

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

        public ushort[] LastScan => throw new NotImplementedException();

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

        public digitalReaderMock(int numberOfChannels)
        {
            _numberOfChennels = numberOfChannels;
        }

        public ushort[] LastScan => throw new NotImplementedException();

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
        public int _numberOfChannels;
        public List<UeiDaq.Channel> _channelList = new List<UeiDaq.Channel>();
        public DigitalWriterMock _digitalWriterMock;
        public digitalReaderMock _digitalReader;
        public AnalogWriterMock _analogWriter;
        public DeviceMock _deviceMock;

        public SessionMock(int numberOfChannels)
        {
            for (int i = 0; i < numberOfChannels; i++)
            {
                //_channelList.Add(new ChannelMock(i));
            }
            _numberOfChannels = numberOfChannels;
        }

        public void Dispose()
        {

        }

        public UeiDaq.Channel GetChannel(int v)
        {
            return _channelList[v];
        }

        public List<UeiDaq.Channel> GetChannels()
        {
            return null;// _channelList;
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
                _digitalReader = new digitalReaderMock(_numberOfChannels);
            }
            return _digitalReader;
        }

        public IWriterAdapter<double[]> GetAnalogScaledReader()
        {
            if (null == _analogWriter)
            {
                _analogWriter = new AnalogWriterMock();
            }
            return _analogWriter;
        }

        DeviceAdapter ISession.GetDevice()
        {
            if (null == _deviceMock)
            {
                _deviceMock = new DeviceMock();
            }
            return null;// _deviceMock;
        }

        public IWriterAdapter<double[]> GetAnalogScaledWriter()
        {
            throw new NotImplementedException();
        }
        UeiDaq.AnalogScaledReader ISession.GetAnalogScaledReader()
        {
            throw new NotImplementedException();
        }

        public CANReaderAdapter GetCANReader(int ch)
        {
            throw new NotImplementedException();
        }

        public bool IsRunning()
        {
            throw new NotImplementedException();
        }

        ICANReaderAdapter ISession.GetCANReader(int ch)
        {
            throw new NotImplementedException();
        }

        List<Channel> ISession.GetChannels()
        {
            throw new NotImplementedException();
        }

        Channel ISession.GetChannel(int v)
        {
            throw new NotImplementedException();
        }

        public DeviceAdapter GetDevice()
        {
            throw new NotImplementedException();
        }
    }
    public class DeviceMock //: IDevice
    {
        public Range[] GetAIRanges()
        {
            throw new NotImplementedException();
        }

        public Range[] GetAORanges()
        {
            Range r = new Range
            {
                maximum = 10.0,
                minimum = 10.0
            };
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

        public ushort[] LastScan => throw new NotImplementedException();

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
    public class AnalogWriterMock1 : IWriterAdapter<double[]>
    {
        public double[] Scan { get; private set; }

        public double[] LastScan => throw new NotImplementedException();

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
    public class ChannelMock //: IChannel
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

        public SerialPortSpeed GetSpeed()
        {
            throw new NotImplementedException();
        }
    }

}
