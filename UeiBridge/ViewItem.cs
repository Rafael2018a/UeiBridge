using System;

/// <summary>
/// All files in project might refer to this file.
/// Types in this file might NOT refer to types in any other file.
/// </summary>
namespace UeiBridge.Library
{
    /// <summary>
    /// Helper class for GetFormattedStatus() method
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ViewItem<T>
    {
        public T ReadValue { get; private set; }
        public TimeSpan TimeToLive { get; private set; }
        public void DecreaseTimeToLive( TimeSpan interval)
        {
            if (TimeToLive > interval)
            {
                TimeToLive -= interval;
            }
            else
            {
                TimeToLive = TimeSpan.Zero;
            }
        }

        public ViewItem(T readValue, TimeSpan timeToLive)
        {
            this.ReadValue = readValue;
            this.TimeToLive = timeToLive;
        }
    }

}
