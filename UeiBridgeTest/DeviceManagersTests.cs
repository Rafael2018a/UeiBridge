using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using UeiBridge;
using UeiBridge.Library;
using UeiDaq;

namespace UeiBridgeTest
{
    [TestFixture]
    class DeviceManagersTests
    {
        [Test]
        //[Category("DeviceManagers")]
        public void BlockSensorTest()
        {

            Session s = new Session();
            s.CreateAOChannel("simu://Dev1/AO0:7", -10, +10);
            writerMock mk = new writerMock();
            mk.OriginSession = s;

            BlockSensorSetup setup = new BlockSensorSetup(new EndPoint("192.168.19.2", 50455), "BlockSensor");
            BlockSensorManager blocksensor = new BlockSensorManager(setup, mk);
            byte[] d403 = StaticMethods.Make_Dio403_upstream_message();
            blocksensor.Enqueue(d403);
            //double factor = AO308Setup.PeekVoltage_downstream / (Int16.MaxValue+1);
            UInt16[] payload = new ushort[14];
            Array.Clear(payload, 0, payload.Length);
            payload[2] = AnalogConverter.PlusMinusVoltageToUInt16(10.0, 5.0);
            payload[9] = AnalogConverter.PlusMinusVoltageToUInt16(10.0, 6.0);
            payload[10] = AnalogConverter.PlusMinusVoltageToUInt16(10.0, 7.0);
            EthernetMessage em = StaticMethods.Make_BlockSensor_downstream_message(payload);
            blocksensor.Enqueue(em.GetByteArray(MessageWay.downstream));
            //s.Stop();
            s.Dispose();
            // 6 7 5

            Assert.Multiple(() =>
            {
                Assert.That(mk.Scan[0], Is.InRange(5.99, 6.01));
                Assert.That(mk.Scan[1], Is.InRange(6.99, 7.01));
                Assert.That(mk.Scan[2], Is.InRange(4.99, 5.01));
            });
        }

        [Test]
        public void AO308DeviceManagerTest()
        {
            Session s = new Session();
            s.CreateAOChannel("simu://Dev1/AO0:7", -10, +10);
            writerMock mk = new writerMock();
            mk.OriginSession = s;
            var list = UeiBridge.Program.BuildDeviceList(new List<string>() { "simu://" });
            var ao = list.Where(i => i.PhDevice.GetDeviceName().StartsWith("Simu-AO16")).FirstOrDefault();
            AO308Setup setup = new AO308Setup(new EndPoint("8.8.8.8", 5000), ao.PhDevice);

            AO308OutputDeviceManager ao308 = new AO308OutputDeviceManager(setup, mk);
            ao308.OpenDevice();
            AnalogConverter ac = new AnalogConverter(10, 12);
            var d1 = new double[] { 5, 7, 9 };
            Byte[] bytes = ac.UpstreamConvert(d1);
            var m = EthernetMessage.CreateMessage(0, 1, 0, bytes);
            ao308.Enqueue(m.GetByteArray(MessageWay.downstream));
            System.Threading.Thread.Sleep(100);
            //s.Stop();
            s.Dispose();

            Assert.Multiple(() => 
            {
                for (int i = 0; i < mk.Scan.Length; i++)
                {
                    Assert.That(mk.Scan[i] - d1[i], Is.InRange(-0.1, 0.1));
                }
            });
        }
    }

    public class writerMock : IWriterAdapter<double[]>
    {
        //public int NumberOfChannels => 8;

        //int IAnalogWrite.NumberOfChannels { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public double[] Scan { get; set; }

        public Session OriginSession { get; set; }

        public void WriteSingleScan(double[] scan)
        {
            Scan = scan;
        }
    }

}
