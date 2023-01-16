using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UeiDaq;

namespace UeiBridge
{
    class SL508Session : IDisposable
    {
        private SL508892Setup _thisDeviceSetup;
        private Session _serialSession;

        public SL508Session(SL508892Setup setup)
        {
            this._thisDeviceSetup = setup;
            this.OpenSession();
        }

        private void OpenSession()
        {
            System.Diagnostics.Debug.Assert(null == _serialSession);
            _serialSession = new Session();
            string finalUrl = $"{_thisDeviceSetup.CubeUrl}Dev{_thisDeviceSetup.SlotNumber}/com0:7";
            // set with default values
            _serialSession.CreateSerialPort(finalUrl,
                SerialPortMode.RS485FullDuplex,
                SerialPortSpeed.BitsPerSecond57600,
                SerialPortDataBits.DataBits8,
                SerialPortParity.None,
                SerialPortStopBits.StopBits1,
                "");

            _serialSession.ConfigureTimingForMessagingIO(100, 10.0);
            _serialSession.GetTiming().SetTimeout(1000);

            System.Diagnostics.Debug.Assert(8 == _serialSession.GetNumberOfChannels());

            _serialSession.Start();

        }

        internal int GetNumberOfChannels()
        {
            return _serialSession.GetNumberOfChannels();
        }

        internal DataStream GetDataStream()
        {
            return _serialSession.GetDataStream();
        }

        internal Channel GetChannel(int ch)
        {
            return _serialSession.GetChannel( ch);
        }

        internal System.Collections.ArrayList GetChannels()
        {
            return _serialSession.GetChannels();
        }

        public void Dispose()
        {
            try
            {
                _serialSession.Stop();
                _serialSession.GetDevice().Reset();
                _serialSession.Dispose();
            }
            catch(Exception ex)
            {
                Console.WriteLine($"SL508Session: {ex.Message}");
            }
        }
    }
}
