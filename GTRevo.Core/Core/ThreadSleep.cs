using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GTRevo.Core.Core
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
