using System;
using System.Threading;
using Revo.Core.Core;

namespace Revo.Testing.Core
{
    public static class FakeClock
    {
        private static readonly ThreadLocal<FakeClockImpl> Clock = new ThreadLocal<FakeClockImpl>(() => new FakeClockImpl());
        private static readonly object setupLock = new object();

        public static DateTimeOffset Now
        {
            get { return Clock.Value.Now; }
            set { Clock.Value.Now = value; }
        }

        public static void Setup()
        {
            lock (setupLock)
            {
                if (!(Revo.Core.Core.Clock.Current is FakeClockImpl))
                {
                    Revo.Core.Core.Clock.SetClock(() => Clock.Value);
                }
            }

            Now = DateTimeOffset.Now;
        }

        public class FakeClockImpl : IClock
        {
            public DateTimeOffset Now { get; set; }
            public DateTimeOffset UtcNow => Now.ToUniversalTime();
        }
    }
}
