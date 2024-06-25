using System;
using Revo.Core.Core;
using Revo.Core.Lifecycle;
using Revo.Infrastructure.Jobs.InMemory;

namespace Revo.Extensions.Notifications.Channels.Buffering
{
    public class BufferedNotificationStartup(IInMemoryJobScheduler inMemoryJobScheduler) : IApplicationStartedListener
    {
        public void OnApplicationStarted()
        {
            var scheduledTime = Clock.Current.UtcNow + TimeSpan.FromMinutes(1);
            inMemoryJobScheduler.EnqeueJobAsync(new ProcessBufferedNotificationsJob(scheduledTime)).Wait();
        }
    }
}
