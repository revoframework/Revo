using System;

namespace Revo.Core.Core
{
    public class SystemClock : IClock
    {
        public DateTimeOffset Now => DateTimeOffset.Now;
        public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
    }
}
