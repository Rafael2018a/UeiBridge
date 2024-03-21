using System;

namespace UeiBridge.Library.Interfaces
{
    /// <summary>
    /// Send items that should be pushed to q (return immediately)
    /// </summary>
    public interface IEnqueue<Item> : IDisposable
    {
        void Enqueue(Item i);
    }


}
