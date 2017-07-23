using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Core.Commands;
using GTRevo.Infrastructure.Notifications.Channels.Push.Commands;
using GTRevo.Infrastructure.Notifications.Channels.Push.Model;
using GTRevo.Infrastructure.Repositories;
using GTRevo.Platform.Security;
using NLog;
using PushSharp.Apple;

namespace GTRevo.Infrastructure.Notifications.Channels.Push.CommandHandlers
{
    public class ExternalPushNotificationCommandHandler :
        IAsyncCommandHandler<RegisterApnsExternalUserDeviceCommand>,
        IAsyncCommandHandler<DeregisterApnsExternalUserDeviceCommand>,
        IAsyncCommandHandler<PushExternalNotificationCommand>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IRepository repository;
        private readonly IApnsBrokerDispatcher apnsBrokerDispatcher;

        public ExternalPushNotificationCommandHandler(IRepository repository,
            IApnsBrokerDispatcher apnsBrokerDispatcher)
        {
            this.repository = repository;
            this.apnsBrokerDispatcher = apnsBrokerDispatcher;
        }

        public async Task Handle(RegisterApnsExternalUserDeviceCommand message)
        {
            string normalizedDeviceToken = message.DeviceToken.Replace(" ", "");
            ApnsExternalUserDeviceToken token = await repository.FirstOrDefaultAsync<ApnsExternalUserDeviceToken>(
                x => x.DeviceToken == normalizedDeviceToken
                     && x.AppId == message.AppId);

            if (token != null)
            {
                if (token.ExternalUserId == message.UserId)
                {
                    return;
                }

                repository.Remove(token);
            }

            // TODO restrict AppIds (a configurable list?)

            token = new ApnsExternalUserDeviceToken(Guid.NewGuid(), message.UserId,
                normalizedDeviceToken.Replace(" ", ""), message.AppId);
            repository.Add(token);

            await repository.SaveChangesAsync();
            Logger.Debug($"Added external APNS external user device token for user ID {message.UserId}");
        }

        public async Task Handle(DeregisterApnsExternalUserDeviceCommand message)
        {
            string normalizedDeviceToken = message.DeviceToken.Replace(" ", "");
            ApnsExternalUserDeviceToken token = await repository.FirstOrDefaultAsync<ApnsExternalUserDeviceToken>(
                x => x.DeviceToken == normalizedDeviceToken
                     && x.AppId == message.AppId);

            if (token != null)
            {
                repository.Remove(token);
                await repository.SaveChangesAsync();
            }
        }

        public async Task Handle(PushExternalNotificationCommand message)
        {
            List<ApnsExternalUserDeviceToken> tokens = await repository.Where<ApnsExternalUserDeviceToken>(
                    x => x.ExternalUserId == message.UserId
                         && x.AppId == message.AppId)
                .ToListAsync();

            if (tokens.Count == 0)
            {
                return;
            }

            apnsBrokerDispatcher.QueueNotifications(
                tokens.Select(
                    x => new ApnsNotification(x.DeviceToken, message.NotificationData)));
        }
    }
}
