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
            BlockSensorManager blocksensor = new BlockSensorManager(setup, new writerMock());
            //blocksensor.Enqueue()
            //Assert.Pass();

        }

    }

    public class writerMock : IAnalogWriter
    {
        public int NumberOfChannels => 8;

        //int IAnalogWrite.NumberOfChannels { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void WriteSingleScan(double[] scen)
        {
            throw new NotImplementedException();
        }
    }
}
