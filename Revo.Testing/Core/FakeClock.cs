using System;
using System.Threading;
using Revo.Core.Core;

namespace Revo.Testing.Core
{
    public static class FakeClock
    {
        private static readonly AsyncLocal<FakeClockImpl> ClockLocal = new();
        private static readonly object SetupLock = new();

        public static DateTimeOffset Now
        {
            get => Clock.Now;
            set => Clock.Now = value;
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
            Now = DateTimeOffset.Now;
            
            lock (SetupLock)
            {
                if (!(Revo.Core.Core.Clock.Current is FakeClockImpl))
                {
                    Revo.Core.Core.Clock.SetClock(() => Clock);
                }
            }
        }

        public class FakeClockImpl : IClock
        {
            private DateTimeOffsetBoxed boxed = new(DateTimeOffset.Now);

            public DateTimeOffset Now
            {
                get => boxed.Value;
                set => boxed = new DateTimeOffsetBoxed(value);
            }
            
            public DateTimeOffset UtcNow => Now.ToUniversalTime();
        }

        private class DateTimeOffsetBoxed(DateTimeOffset value)
        {
            public DateTimeOffset Value { get; } = value;
        }
    }
}
