using System;
using System.Collections.Generic;
using UeiBridge.Interfaces;
using UeiDaq;

namespace UeiBridge.Library
{
    public interface ISession : IDisposable
    {
        List<IChannel> GetChannels();
        IChannel GetChannel(int v);
        //DataStream GetDataStream();
        IReaderAdapter<UInt16[]> GetDigitalReader();
        IWriterAdapter<UInt16[]> GetDigitalWriter();
        //void Stop();
        int GetNumberOfChannels();
        IDevice GetDevice();
        IReaderAdapter<double[]> GetAnalogScaledReader();
        IWriterAdapter<double[]> GetAnalogScaledWriter();
        ICANReaderAdapter GetCANReader(int ch);
        bool IsRunning();
        void Stop();
    }
}
