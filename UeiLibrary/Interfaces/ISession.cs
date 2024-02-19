using System;
using System.Collections.Generic;
using UeiBridge.Library.Interfaces;
using UeiDaq;

namespace UeiBridge.Library
{
    public interface ISession : IDisposable
    {
        List<UeiDaq.Channel> GetChannels();
        UeiDaq.Channel GetChannel(int v);
        //DataStream GetDataStream();
        IReaderAdapter<UInt16[]> GetDigitalReader();
        IWriterAdapter<UInt16[]> GetDigitalWriter();
        //void Stop();
        int GetNumberOfChannels();
        DeviceAdapter GetDevice();
        UeiDaq.AnalogScaledReader GetAnalogScaledReader();
        IWriterAdapter<double[]> GetAnalogScaledWriter();
        ICANReaderAdapter GetCANReader(int ch);
        bool IsRunning();
        void Stop();
    }
}
