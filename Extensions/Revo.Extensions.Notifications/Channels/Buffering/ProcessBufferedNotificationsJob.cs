using System;
using Revo.Infrastructure.Jobs;

namespace Revo.Extensions.Notifications.Channels.Buffering
{
    public class ProcessBufferedNotificationsJob : IJob
    {
        public ProcessBufferedNotificationsJob(DateTimeOffset scheduledTime)
        {
            ScheduledTime = scheduledTime;
        }

        public DateTimeOffset ScheduledTime { get; }
    }
}
