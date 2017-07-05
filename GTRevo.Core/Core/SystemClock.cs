using System;

namespace GTRevo.Core.Core
{
    public class SystemClock : IClock
    {
        public DateTime Now => DateTime.Now;
        public DateTime UtcNow => DateTime.UtcNow;
    }
}
