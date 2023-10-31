using System;

namespace UeiBridge.Interfaces
{
    public interface ISend<Item> : IDisposable
    {
        void Send(Item i);
    }


}
