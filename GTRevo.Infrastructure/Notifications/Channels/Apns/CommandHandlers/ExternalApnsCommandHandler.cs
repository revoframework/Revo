using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GTRevo.Core.Commands;
using GTRevo.Infrastructure.Notifications.Channels.Apns.Commands;
using GTRevo.Infrastructure.Notifications.Channels.Apns.Model;
using GTRevo.Infrastructure.Repositories;
using NLog;
using PushSharp.Apple;

namespace GTRevo.Infrastructure.Notifications.Channels.Apns.CommandHandlers
{
    public class ExternalApnsNotificationCommandHandler :
        ICommandHandler<RegisterApnsExternalUserDeviceCommand>,
        ICommandHandler<DeregisterApnsExternalUserDeviceCommand>,
        ICommandHandler<PushExternalApnsNotificationCommand>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IRepository repository;
        private readonly IApnsBrokerDispatcher apnsBrokerDispatcher;

        public ExternalApnsNotificationCommandHandler(IRepository repository,
            IApnsBrokerDispatcher apnsBrokerDispatcher)
        {
            this.repository = repository;
            this.apnsBrokerDispatcher = apnsBrokerDispatcher;
        }

        public async Task HandleAsync(RegisterApnsExternalUserDeviceCommand message, CancellationToken cancellationToken)
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

        public async Task HandleAsync(DeregisterApnsExternalUserDeviceCommand message, CancellationToken cancellationToken)
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

        public async Task HandleAsync(PushExternalApnsNotificationCommand message, CancellationToken cancellationToken)
        {
            List<ApnsExternalUserDeviceToken> tokens = await repository.Where<ApnsExternalUserDeviceToken>(
                    x => message.UserIds.Contains(x.ExternalUserId)
                         && x.AppId == message.AppId)
                .ToListAsync();

            if (tokens.Count == 0)
            {
                return;
            }

            apnsBrokerDispatcher.QueueNotifications(
                tokens.Select(
                    x => new WrappedApnsNotification(new ApnsNotification(x.DeviceToken, message.Payload), x.AppId)));
        }
    }
}
