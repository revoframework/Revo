using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Revo.Infrastructure.Notifications;
using Revo.Infrastructure.Notifications.Channels.Apns;
using Newtonsoft.Json.Linq;
using NSubstitute;
using PushSharp.Apple;
using Xunit;

namespace Revo.Infrastructure.Tests.Notifications.Channels.Apns
{
    public class ApnsBufferedNotificationChannelTests
    {
        private readonly ApnsBufferedNotificationChannel sut;
        private IApnsNotificationFormatter[] pushNotificationFormatters;
        private IApnsBrokerDispatcher apnsBrokerDispatcher;

        public ApnsBufferedNotificationChannelTests()
        {
            pushNotificationFormatters = new[]
            {
                Substitute.For<IApnsNotificationFormatter>(),
                Substitute.For<IApnsNotificationFormatter>()
            };
            
            apnsBrokerDispatcher = Substitute.For<IApnsBrokerDispatcher>();

            sut = new ApnsBufferedNotificationChannel(pushNotificationFormatters, apnsBrokerDispatcher);
        }

        [Fact]
        public async Task SendNotificationsAsync_FormatsAndQueues()
        {
            var notifications = new List<TestNotification>()
            {
                new TestNotification(),
                new TestNotification()
            };

            var apnsNotifications = new List<WrappedApnsNotification>()
            {
                new WrappedApnsNotification(new ApnsNotification("1cc31e97c96b274eb056a969eff7f6d783fcdf6985dc457734837bd45fb1a731", new JObject()), "my.appid"),
                new WrappedApnsNotification(new ApnsNotification("1da31e97c96b274eb056a969eff7f6d783fcdf6985dc457734837bd45fb1a731", new JObject()), "my.appid")
            };

            pushNotificationFormatters[0].FormatPushNotification(Arg.Is<IEnumerable<INotification>>(
                    x => x.Count() == notifications.Count && x.All(notifications.Contains)))
                .Returns(apnsNotifications.Take(1));
            pushNotificationFormatters[1].FormatPushNotification(Arg.Is<IEnumerable<INotification>>(
                    x => x.Count() == notifications.Count && x.All(notifications.Contains)))
                .Returns(apnsNotifications.Skip(1));

            await sut.SendNotificationsAsync(notifications);

            //we want them to get queued efficiently, all at once
            apnsBrokerDispatcher.Received(1).QueueNotifications(Arg.Is<IEnumerable<WrappedApnsNotification>>(
                x => x.Count() == apnsNotifications.Count && x.All(apnsNotifications.Contains)));
        }

        public class TestNotification : INotification
        {
        }
    }
}
