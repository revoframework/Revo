using System;
using System.Threading;
using GTRevo.Core.Core;

namespace GTRevo.Testing.Core
{
    public static class FakeClock
    {
        private static readonly ThreadLocal<FakeClockImpl> Clock = new ThreadLocal<FakeClockImpl>(() => new FakeClockImpl());
        
        public static DateTimeOffset Now
        {
            get { return Clock.Value.Now; }
            set { Clock.Value.Now = value; }
        }

        public static void Setup()
        {
            GTRevo.Core.Core.Clock.SetClock(() => Clock.Value);
            Now = DateTimeOffset.Now;
        }

        public class FakeClockImpl : IClock
        {
            public DateTimeOffset Now { get; set; }
            public DateTimeOffset UtcNow => Now.ToUniversalTime();
        }
    }
}
