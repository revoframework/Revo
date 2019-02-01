using System;
using System.Threading;
using Revo.Core.Core;

namespace Revo.Testing.Core
{
    public static class FakeClock
    {
        private static readonly AsyncLocal<FakeClockImpl> ClockLocal = new AsyncLocal<FakeClockImpl>();
        private static readonly object SetupLock = new object();

        public static DateTimeOffset Now
        {
            get { return Clock.Now; }
            set { Clock.Now = value; }
        }

        private static FakeClockImpl Clock
        {
            get
            {
                if (ClockLocal.Value == null)
                {
                    ClockLocal.Value = new FakeClockImpl();
                }

                return ClockLocal.Value;
            }
        }
        
        public static void Setup()
        {
            lock (SetupLock)
            {
                if (!(Revo.Core.Core.Clock.Current is FakeClockImpl))
                {
                    Revo.Core.Core.Clock.SetClock(() => Clock);
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
