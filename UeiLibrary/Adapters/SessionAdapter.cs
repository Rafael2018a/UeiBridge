using System;
using System.Collections.Generic;
using System.Linq;
using UeiDaq;

namespace UeiBridge.Library
{
    public class SessionAdapter : ISession
    {
        Session _ueiSession;
        DigitalReaderAdapter _digitalReader;
        DigitalWriterAdapter _digitalWriter;
        AnalogScaledWriteAdapter _analogScaledWriter;
        //AnalogScaledReaderAdapter _analogScaledReader;

        public SessionAdapter(Session ueiSession)
        {
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

        public IWriterAdapter<double[]> GetAnalogScaledReader()
        {
            //if (null==_analogScaledWriter)
            throw new NotImplementedException();
        }

        public IWriterAdapter<double[]> GetAnalogScaledWriter()
        {
            if (null==_analogScaledWriter)
            {
                _analogScaledWriter = new AnalogScaledWriteAdapter( new AnalogScaledWriter( _ueiSession.GetDataStream()));
            }
            return _analogScaledWriter;
        }

        public IReaderAdapter<UInt16[]> GetDigitalReader()
        {
            if (null==_digitalReader)
            {
                _digitalReader = new DigitalReaderAdapter( new DigitalReader(_ueiSession.GetDataStream()));
            }
            return _digitalReader;
        }

        public IWriterAdapter<UInt16[]> GetDigitalWriter()
        {
            if (null==_digitalWriter)
            {
                _digitalWriter = new DigitalWriterAdapter(new DigitalWriter(_ueiSession.GetDataStream()));
            }
            return _digitalWriter;
        }

        public int GetNumberOfChannels()
        {
            return _ueiSession.GetNumberOfChannels();
        }


        IChannel ISession.GetChannel(int v)
        {
            return new ChannelAdapter( _ueiSession.GetChannel(v));
        }

        List<IChannel> ISession.GetChannels()
        {
            var r = _ueiSession.GetChannels().Cast<Channel>().ToList().Select(i => new ChannelAdapter(i));
            return r.ToList<IChannel>();
        }

        IDevice ISession.GetDevice()
        {
            return new DeviceAdapter( _ueiSession.GetDevice());
        }
    }




}
