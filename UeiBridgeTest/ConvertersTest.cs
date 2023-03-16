﻿using System;
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
        public void TestValidVoltageConversion(double v)
        {
            UInt16 u16 = AnalogConverter.PlusMinusVoltageToUInt16(10.0, v);
            double v1 = AnalogConverter.Uint16ToPlusMinusVoltage(10.0, u16);
            Assert.That(v1 - v, Is.InRange(-0.1, 0.1));
        }
        [TestCase(20.0)]
        public void TestInvalidVoltageConversion(double v)
        {
            double peek = 10;
            UInt16 u16 = AnalogConverter.PlusMinusVoltageToUInt16(peek, v);
            double v1 = AnalogConverter.Uint16ToPlusMinusVoltage(peek, u16);
            Assert.That(v1 - peek, Is.InRange(-0.1, 0.1));
        }

        [Test]
        public void AnalogConverterDownstreamTest()
        {
            var c = new AnalogConverter(AI201100Setup.PeekVoltage_upstream, AO308Setup.PeekVoltage_downstream);
            UInt16 d1v_ds = Convert.ToUInt16(UInt16.MaxValue / AO308Setup.PeekVoltage_downstream / 2.0);
            UInt16 zero_v = Convert.ToUInt16(d1v_ds * AO308Setup.PeekVoltage_downstream);
            byte[] a = new byte[2];
            Array.Copy(BitConverter.GetBytes(zero_v), a, 2);
            double[] d = c.DownstreamConvert(a);

            Assert.That(d[0], Is.InRange(-0.1, 0.1));
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

    }
}
