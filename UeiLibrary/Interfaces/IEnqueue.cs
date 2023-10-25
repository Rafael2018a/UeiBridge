namespace UeiBridge.Interfaces
{
    /// <summary>
    /// Send items that should be pushed to q (return immediately)
    /// </summary>
    public interface IEnqueue<Item>
    {
        void Enqueue(Item i);
    }


}
