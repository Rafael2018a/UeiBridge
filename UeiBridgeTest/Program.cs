using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using UeiBridge;
using UeiBridge.Library;

namespace UeiBridgeTest
{
    [TestFixture]
    class Program
    {
        static void Main(string[] args)
        {
            Program p = new Program();
            p.BlockSensorTest();
        }

        [Test]
        public void BlockSensorTest()
        {
            BlockSensorSetup setup = new BlockSensorSetup(new EndPoint("192.168.19.2", 50455), "BlockSensor");
            writerMock mk = new writerMock();
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

            var s = mk.Scan;
        }

        [TestCase(10.01)]
        [TestCase(-10.01)]
        [TestCase(9.0)]
        [TestCase(-9.0)]
        [TestCase(1.0)]
        [TestCase(-1.0)]
        public void VoltageConversionTest1(double v)
        {
            UInt16 u16 = AnalogConverter.PlusMinusVoltageToUInt16(10.0, v);
            double v1 = AnalogConverter.Uint16ToPlusMinusVoltage(10.0, u16);
            Assert.That( v1, Is.InRange(v - 0.1, v + 0.1));
        }
        [Test]
        public void VoltageConversionTest2()
        {
            UInt16 u16 = AnalogConverter.PlusMinusVoltageToUInt16(10.0, -10.0);
            Assert.That(u16, Is.EqualTo(0));
        }

        [Test]
        public void VoltageConversionTest3()
        {
            double d = AnalogConverter.Uint16ToPlusMinusVoltage(10.0, UInt16.MaxValue - 10);
            Assert.That(d, Is.GreaterThan(9.9));
        }
        [Test]
        public void VoltageConversionTest4()
        {
            double d = AnalogConverter.Uint16ToPlusMinusVoltage(10.0, 10);
            Assert.That(d, Is.LessThan(-9.9));
        }

    }

    public class writerMock : IAnalogWriter
    {
        public int NumberOfChannels => 8;

        //int IAnalogWrite.NumberOfChannels { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public double [] Scan { get; set; }
        public void WriteSingleScan(double[] scan)
        {
            Scan = scan;
        }
    }
}
