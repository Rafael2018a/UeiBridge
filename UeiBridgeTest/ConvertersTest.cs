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
        //[TestCase(10.01)]
        //[TestCase(-10.01)]
        //[TestCase(9.0)]
        //[TestCase(-9.0)]
        //[TestCase(1.0)]
        //[TestCase(-1.0)]
        //public void TestValidVoltageConversion(double v)
        //{
        //    UInt16 u16 = AnalogConverter.PlusMinusVoltageToUInt16(10.0, v);
        //    double v1 = AnalogConverter.Uint16ToPlusMinusVoltage(10.0, u16);
        //    Assert.That(v1 - v, Is.InRange(-0.1, 0.1));
        //}
        //[TestCase(20.0)]
        //public void TestInvalidVoltageConversion(double v)
        //{
        //    double peek = 10;
        //    UInt16 u16 = AnalogConverter.PlusMinusVoltageToUInt16(peek, v);
        //    double v1 = AnalogConverter.Uint16ToPlusMinusVoltage(peek, u16);
        //    Assert.That(v1 - peek, Is.InRange(-0.1, 0.1));
        //}

        [Test]
        public void AnalogConverterDownstreamTest() // short to double
        {
            var c = new AnalogConverter(AI201100Setup.PeekVoltage_upstream, AO308Setup.PeekVoltage_downstream);
            List<Int16> Int16List = new List<short>();
            for(int val=0; val < Int16.MaxValue; val+= Int16.MaxValue/10)
            {
                Int16List.Add(  Convert.ToInt16( val));
            }
            byte[] byteArray = new byte[Int16List.Count * 2];
            for(int i = 0; i<Int16List.Count; i++)
            {
                byte[] t = BitConverter.GetBytes(Int16List[i]);
                Array.Copy(t, 0, byteArray, i * 2, 2);
            }
            double[] d = c.DownstreamConvert(byteArray);

            double dval = 0;
            for (int i = 0; i < Int16List.Count; i++)
            {
                Assert.That(d[i], Is.InRange(dval - 0.1, dval + 0.1));
                dval += 1.0;
            }
        }

        [Test]
        public void AnalogConverter2wayTest()
        {
            var c = new AnalogConverter(10, 10);
            double[] d1 = new double[] { -6, -3, 0, 3, 6 };
            byte [] bytes = c.UpstreamConvert(d1);
            double [] d2 = c.DownstreamConvert(bytes);

            Assert.Multiple(() =>
            {
                for(int i=0; i<d1.Length; i++)
                {
                    Assert.That(d1[i] - d2[i], Is.InRange(-0.1, 01));
                }
            });
        }

        [Test]
        public void VoltageOutputConvertTest()
        {
            double dp = AnalogConverter.Int16ToPlusMinusVoltage(10.0, 32767);
            Assert.That(dp, Is.InRange(9.9, 10.1));

            double dn = AnalogConverter.Int16ToPlusMinusVoltage(10.0, -32767);
            Assert.That(dn, Is.InRange(-10.1, -9.9));

            double d = AnalogConverter.Int16ToPlusMinusVoltage(10.0, 1);
            Assert.That(d, Is.InRange(-0.1, 0.1));

        }
        [Test]
        public void VoltageInputConvertTest()
        {
            Int16 i16 = AnalogConverter.PlusMinusVoltageToInt16(12.0, 12.0);
            Assert.That(i16, Is.InRange(32760, 32770));
            // check clipping
            i16 = AnalogConverter.PlusMinusVoltageToInt16(12.0, 12.01);
            Assert.That(i16, Is.InRange(32760, 32770));

            i16 = AnalogConverter.PlusMinusVoltageToInt16(12.0, -12.0);
            Assert.That(i16, Is.InRange(-32770, -32760));

            i16 = AnalogConverter.PlusMinusVoltageToInt16(12.0, -12.01);
            Assert.That(i16, Is.InRange(-32770, -32760));
        }
    }
}
