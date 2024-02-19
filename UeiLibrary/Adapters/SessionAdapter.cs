using System;
using System.Collections.Generic;
using System.Linq;
using UeiBridge.Library.Interfaces;
using UeiDaq;

namespace UeiBridge.Library
{
    /// <summary>
    /// This class wraps ueidaq.Session object
    /// It also creates and return session readers/writers upon need.
    /// </summary>
    public class SessionAdapter : ISession
    {
        readonly Session _ueiSession;
        DigitalReaderAdapter _digitalReaderAd;
        DigitalWriterAdapter _digitalWriterAd;
        AnalogScaledWriteAdapter _analogWriterAd;
        AnalogScaledReaderAdapter _analogReaderAd;

        public SessionAdapter(Session ueiSession)
        {
            if (false == ueiSession.IsRunning())
            {
                Console.WriteLine($"Session not running");
            }
            _ueiSession = ueiSession;
        }
        public void Dispose()
        {
            _ueiSession.Dispose();
        }
        public IReaderAdapter<double[]> GetAnalogScaledReaderOrig()
        {
            if (null == _analogReaderAd)
            {
                _analogReaderAd = new AnalogScaledReaderAdapter(new AnalogScaledReader(_ueiSession.GetDataStream()));
            }
            return _analogReaderAd;
        }
        public UeiDaq.AnalogScaledReader GetAnalogScaledReader()
        {
                return new AnalogScaledReader(_ueiSession.GetDataStream());
        }
        public IWriterAdapter<double[]> GetAnalogScaledWriter()
        {
            if (null==_analogWriterAd)
            {
                _analogWriterAd = new AnalogScaledWriteAdapter( new AnalogScaledWriter( _ueiSession.GetDataStream()));
            }
            return _analogWriterAd;
        }
        public IReaderAdapter<UInt16[]> GetDigitalReader()
        {
            if (null==_digitalReaderAd)
            {
                _digitalReaderAd = new DigitalReaderAdapter( new DigitalReader(_ueiSession.GetDataStream()));
            }
            return _digitalReaderAd;
        }

        public Device GetAssociatedDevice()
        {
            return _ueiSession.GetDevice();
        }

        public CANWriterAdapter GetCANWriter(int ch)
        {
            return new CANWriterAdapter(new CANWriter(_ueiSession.GetDataStream(), _ueiSession.GetChannel(ch).GetIndex()));
        }

        public ICANReaderAdapter GetCANReader(int ch)
        {
            return new CANReaderAdapter( new CANReader(_ueiSession.GetDataStream(), _ueiSession.GetChannel(ch).GetIndex()));
        }

        public IWriterAdapter<UInt16[]> GetDigitalWriter()
        {
            if (null==_digitalWriterAd)
            {
                _digitalWriterAd = new DigitalWriterAdapter(new DigitalWriter(_ueiSession.GetDataStream()));
            }
            return _digitalWriterAd;
        }
        public int GetNumberOfChannels()
        {
            return _ueiSession.GetNumberOfChannels();
        }
       public List<UeiDaq.Channel> GetChannels() 
        {
            var r = _ueiSession.GetChannels().Cast<UeiDaq.Channel>().ToList();//.Select(i => new ChannelAdapter(i));
            return r;
        }
        DeviceAdapter ISession.GetDevice()
        {
            return new DeviceAdapter( _ueiSession.GetDevice());
        }

        public UeiDaq.Channel GetChannel(int serialChannelNumber)
        {
            return GetChannels()[serialChannelNumber];
        }

        public bool IsRunning()
        {
            return _ueiSession.IsRunning();
        }

        public DataStream GetDataStream()
        {
            return _ueiSession.GetDataStream();
        }

        public void Stop()
        {
            try
            {
                if (_ueiSession.IsRunning())
                {
                    _ueiSession.Stop();
                }
            }
            catch (UeiDaq.UeiDaqException ex)
            {
                Console.WriteLine($"Session stop() failed. {ex.Message}");
            }
        }
        public SerialReader GetSerialReader(int ch)
        {
           var sr = new SerialReader(_ueiSession.GetDataStream(), _ueiSession.GetChannel(ch).GetIndex());
            return sr;
        }
    }




}
