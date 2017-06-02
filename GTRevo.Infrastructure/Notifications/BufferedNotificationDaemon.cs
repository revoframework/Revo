using GTRevo.DataAccess.EF6;
using GTRevo.Platform.Core.Lifecycle;
using Hangfire;

namespace GTRevo.Infrastructure.Notifications
{
    public class BufferedNotificationDaemon : IApplicationStartListener
    {
        public BufferedNotificationDaemon()
        {
        }

        public void OnApplicationStarted()
        {
            Run();
        }

        public void Run()
        {
            RecurringJob.AddOrUpdate<BufferedNotificationProcessJob>(
                bufferedNotificationProcessJob => bufferedNotificationProcessJob.Run(),
                Cron.Minutely);

            // TODO prune empty notifications buffers
        }

        public void RunOnce()
        {
            BackgroundJob.Enqueue<BufferedNotificationProcessJob>(
                bufferedNotificationProcessJob => bufferedNotificationProcessJob.Run());
        }
    }
}
