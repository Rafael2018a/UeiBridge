using System;
using System.Collections.Generic;
using System.Linq;
using UeiDaq;

namespace UeiBridge.Library
{
    public class UeiSessionAdapter : ISession
    {
        Session _ueiSession;

        public UeiSessionAdapter(Session ueiSession)
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

        public IReaderAdapter<UInt16[]> GetDigitalReader()
        {
            UeiDigitalReaderAdapter dr = new UeiDigitalReaderAdapter( new DigitalReader(_ueiSession.GetDataStream()));
            return dr;
        }

        public IWriterAdapter<UInt16[]> GetDigitalWriter()
        {
            UeiDigitalWriterAdapter dw = new UeiDigitalWriterAdapter(new DigitalWriter(_ueiSession.GetDataStream()));
            return dw;
        }

        public int GetNumberOfChannels()
        {
            return _ueiSession.GetNumberOfChannels();
        }


        IChannel ISession.GetChannel(int v)
        {
            return new UeiChannelAdapter( _ueiSession.GetChannel(v));
        }

        List<IChannel> ISession.GetChannels()
        {
            var r = _ueiSession.GetChannels().Cast<Channel>().ToList().Select(i => new UeiChannelAdapter(i));
            return r.ToList<IChannel>();
        }
    }




}
