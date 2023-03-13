using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UeiDaq;

namespace UeiBridge
{
    public class SL508Session : IDisposable
    {
        private SL508892Setup _thisDeviceSetup;
        private Session _serialSession = new Session();

        public Session SerialSession => _serialSession;

        public int SlotNumber { get { return _thisDeviceSetup.SlotNumber; } }

        public SL508Session(SL508892Setup setup)
        {
            System.Diagnostics.Debug.Assert(null != setup);
            this._thisDeviceSetup = setup;
            this.OpenSession();
        }
        private void OpenSession()
        {
            System.Diagnostics.Debug.Assert(null != _thisDeviceSetup);
            //Session serialSession = new Session();
            log4net.ILog _logger = StaticMethods.GetLogger();
            try
            {
                foreach (var channel in _thisDeviceSetup.Channels)
                {
                    string finalUrl = $"{_thisDeviceSetup.CubeUrl}Dev{_thisDeviceSetup.SlotNumber}/{channel.portname}";
                    var port = _serialSession.CreateSerialPort(finalUrl,
                                        channel.mode,
                                        channel.Baudrate,
                                        SerialPortDataBits.DataBits8,
                                        channel.parity,
                                        channel.stopbits,
                                        "");
                }

                System.Diagnostics.Debug.Assert(_serialSession.GetNumberOfChannels() == _thisDeviceSetup.Channels.Length);
                //System.Diagnostics.Debug.Assert(numberOfChannels == Config.Instance.SerialChannels.Length);

                _serialSession.ConfigureTimingForMessagingIO(1000, 100.0);
                _serialSession.GetTiming().SetTimeout(5000); // timeout to throw from _serialReader.EndRead (looks like default is 1000)

                //serialSession.ConfigureTimingForSimpleIO();

                _serialSession.Start();

                _logger.Info( $" == Serial channels for cube {_thisDeviceSetup.CubeUrl}, slot {this.SlotNumber}");
                foreach (Channel c in _serialSession.GetChannels())
                {
                    SerialPort sp1 = c as SerialPort;
                    _logger.Debug($"CH:{sp1.GetIndex()}, Speed:{sp1.GetSpeed()}, Mode: {sp1.GetMode()}");
                }
            }
            catch(UeiDaqException ex)
            {
                _logger.Warn($"Failed to init serial card mananger.Slot {_thisDeviceSetup.SlotNumber}. {ex.Message}. Might be invalid baud rate");
                _serialSession = null;
            }
            catch( Exception ex)
            {
                _logger.Warn(ex.Message);
            }

        }
        private void OpenSession_Brian()
        {
            log4net.ILog _logger = StaticMethods.GetLogger();

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
            _serialSession.GetTiming().SetTimeout(5);

            //int ch = 0;
            int chIdx = _serialSession.GetChannel(4).GetIndex();
            SerialPort sp = (SerialPort)_serialSession.GetChannel(chIdx);
            sp.SetSpeed(SerialPortSpeed.BitsPerSecond115200);
            chIdx = _serialSession.GetChannel(5).GetIndex();
            sp = (SerialPort)_serialSession.GetChannel(chIdx);
            sp.SetSpeed(SerialPortSpeed.BitsPerSecond38400);

            List<SerialPort> ports = new List<SerialPort>();
            foreach ( Channel c in _serialSession.GetChannels())
            {
                SerialPort sp1 = c as SerialPort;
                ports.Add(sp1);


                _logger.Debug($"CH{sp1.GetIndex() }, Speed{sp1.GetSpeed()}");
                ;
                
            }


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
            if (null!=this._serialSession)
            {
                _serialSession.Stop();
                _serialSession.GetDevice().Reset();
                _serialSession.Dispose();
            }
        }
    }
}
// Allowed rates, from Brian:
// DQ_SL501_BAUD_300           (1L)
// DQ_SL501_BAUD_600           (2L)
// DQ_SL501_BAUD_1200          (3L)
// DQ_SL501_BAUD_2400          (4L)
// DQ_SL501_BAUD_4800          (5L)
// DQ_SL501_BAUD_9600          (6L)
// DQ_SL501_BAUD_19200         (7L)
// DQ_SL501_BAUD_38400         (8L)
// DQ_SL501_BAUD_56000         (9L)
// DQ_SL501_BAUD_115200        (10L)
// DQ_SL501_BAUD_128000        (11L)
// DQ_SL501_BAUD_250000        (12L)
// DQ_SL501_BAUD_256000        (13L)
// DQ_SL501_BAUD_1000000       (14L)

/* from metadata:
public enum SerialPortSpeed
{
    BitsPerSecond110 = 0,
    BitsPerSecond300 = 1,
    BitsPerSecond600 = 2,
    BitsPerSecond1200 = 3,
    BitsPerSecond2400 = 4,
    BitsPerSecond4800 = 5,
    BitsPerSecond9600 = 6,
    BitsPerSecond14400 = 7,
    BitsPerSecond19200 = 8,
    BitsPerSecond28800 = 9,
    BitsPerSecond38400 = 10,
    BitsPerSecond57600 = 11,
    BitsPerSecond115200 = 12,
    BitsPerSecond128000 = 13,
    BitsPerSecond250000 = 14,
    BitsPerSecond256000 = 15,
    BitsPerSecond1000000 = 16,
    BitsPerSecondCustom = 17
}
*/

// from example program of Brian:
//myPort[chIndex].SetCustomSpeed(115200);
//myPort[chIndex].SetSpeed(SerialPortSpeed.BitsPerSecond38400);
//myPort[chIndex].SetSpeed(SerialPortSpeed.BitsPerSecond128000);
//myPort[chIndex].SetSpeed(SerialPortSpeed.BitsPerSecond250000);

