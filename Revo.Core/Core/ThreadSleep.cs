using System;
using System.Threading;
using System.Threading.Tasks;

namespace Revo.Core.Core
{
    public class ThreadSleep : ISleep
    {
        public void Sleep(TimeSpan timeout)
        {
            Thread.Sleep(timeout);
        }

        public Task SleepAsync(TimeSpan timeout)
        {
            return Task.Delay(timeout);
        }
    }
}
