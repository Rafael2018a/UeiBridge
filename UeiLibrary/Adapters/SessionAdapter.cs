using System;
using System.Collections.Generic;
using System.Linq;
using UeiDaq;

namespace UeiBridge.Library
{
    /// <summary>
    /// This class wraps ueidaq.Session object
    /// It also creates and return session readers/writers upon need.
    /// </summary>
    public class SessionAdapter : ISession
    {
        Session _ueiSession;
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
            try
            {
                _ueiSession.Stop();
            }
            catch (UeiDaq.UeiDaqException ex)
            {
                Console.WriteLine($"Session stop() failed. {ex.Message}");
            }
            _ueiSession.Dispose();
        }
        public IReaderAdapter<double[]> GetAnalogScaledReader()
        {
            if (null == _analogReaderAd)
            {
                _analogReaderAd = new AnalogScaledReaderAdapter( new AnalogScaledReader( _ueiSession.GetDataStream()));
            }
            return _analogReaderAd;
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
       public List<IChannel> GetChannels() 
        {
            var r = _ueiSession.GetChannels().Cast<Channel>().ToList().Select(i => new ChannelAdapter(i));
            return r.ToList<IChannel>();
        }
        IDevice ISession.GetDevice()
        {
            return new DeviceAdapter( _ueiSession.GetDevice());
        }

        public IChannel GetChannel(int serialChannelNumber)
        {
            return new ChannelAdapter(_ueiSession.GetChannel( serialChannelNumber));
        }

    }




}
