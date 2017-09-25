using System;
using System.Threading;
using GTRevo.Core.Core;

namespace GTRevo.Testing.Core
{
    public static class FakeClock
    {
        private static readonly ThreadLocal<FakeClockImpl> Clock = new ThreadLocal<FakeClockImpl>(() => new FakeClockImpl());

        public static DateTime Now
        {
            get { return Clock.Value.Now; }
            set { Clock.Value.Now = value; }
        }

        public static DateTime UtcNow
        {
            get { return Clock.Value.UtcNow; }
            set { Clock.Value.UtcNow = value; }
        }

        public static void Setup()
        {
            GTRevo.Core.Core.Clock.SetClock(() => Clock.Value);
            Now = DateTime.Now;
            UtcNow = Now.ToUniversalTime();
        }

        public class FakeClockImpl : IClock
        {
            public DateTime Now { get; set; }
            public DateTime UtcNow { get; set; }
        }
    }
}
