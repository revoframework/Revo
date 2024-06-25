using System;
using Revo.Infrastructure.Jobs;

namespace Revo.Extensions.Notifications.Channels.Buffering
{
    public class ProcessBufferedNotificationsJob(DateTimeOffset scheduledTime) : IJob
    {
        public DateTimeOffset ScheduledTime { get; } = scheduledTime;
    }
}
