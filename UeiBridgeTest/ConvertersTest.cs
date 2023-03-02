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
    class ConvertersTest
    {
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
            Assert.That(v1 - v, Is.InRange(-0.1, 0.1));
        }

        [Test]
        public void AnalogConverterTest()
        {
            var c = new AnalogConverter(10, 12);
            UInt16 u16 = UInt16.MaxValue / 2;
            byte[] two = new byte[2];
            Array.Copy(BitConverter.GetBytes(u16), two, 2);
            double[] d = c.DownstreamConvert(two);

            Assert.That(d[0], Is.InRange(-0.1, 0.1));

        }

    }
}
