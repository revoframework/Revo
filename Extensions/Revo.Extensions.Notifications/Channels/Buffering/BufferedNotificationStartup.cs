using System;
using System.Collections.Generic;
using System.Text;
using Revo.Core.Core;
using Revo.Core.Lifecycle;
using Revo.Infrastructure.Jobs;
using Revo.Infrastructure.Jobs.InMemory;

namespace Revo.Extensions.Notifications.Channels.Buffering
{
    public class BufferedNotificationStartup : IApplicationStartedListener
    {
        private readonly IInMemoryJobScheduler inMemoryJobScheduler;

        public BufferedNotificationStartup(IInMemoryJobScheduler inMemoryJobScheduler)
        {
            this.inMemoryJobScheduler = inMemoryJobScheduler;
        }

        public void OnApplicationStarted()
        {
            var scheduledTime = Clock.Current.Now + TimeSpan.FromMinutes(1);
            inMemoryJobScheduler.EnqeueJobAsync(new ProcessBufferedNotificationsJob(scheduledTime)).Wait();
        }
    }
}
