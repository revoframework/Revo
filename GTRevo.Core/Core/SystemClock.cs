using System;

namespace GTRevo.Core.Core
{
    public class SystemClock : IClock
    {
        public DateTimeOffset Now => DateTimeOffset.Now;
        public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
    }
}
