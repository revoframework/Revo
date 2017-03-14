using System;

namespace GTRevo.Platform.Core
{
    public class SystemClock : IClock
    {
        public DateTime Now
        {
            get
            {
                return DateTime.Now;
            }
        }
        public DateTime UtcNow
        {
            get
            {
                return DateTime.UtcNow;
            }
        }
    }
}
