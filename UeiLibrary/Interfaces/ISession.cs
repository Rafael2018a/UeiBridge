﻿using System;
using System.Collections.Generic;

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
    }




}
