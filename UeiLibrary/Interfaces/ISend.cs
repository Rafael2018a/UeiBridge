using System;

namespace UeiBridge.Library.Interfaces
{
    public interface ISend<Item> : IDisposable
    {
        void Send(Item i);
    }


}
