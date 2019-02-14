using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using PushSharp.Apple;
using Revo.Core.Commands;
using Revo.DataAccess.Entities;
using Revo.Extensions.Notifications.Channels.Apns.Commands;
using Revo.Extensions.Notifications.Channels.Apns.Model;
using Revo.Infrastructure.Repositories;

namespace Revo.Extensions.Notifications.Channels.Apns.Handlers
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
            }
        }

        public async Task HandleAsync(PushExternalApnsNotificationCommand message, CancellationToken cancellationToken)
        {
            List<ApnsExternalUserDeviceToken> tokens = await repository.Where<ApnsExternalUserDeviceToken>(
                    x => message.UserIds.Contains(x.ExternalUserId)
                         && x.AppId == message.AppId)
                .ToListAsync(repository.GetQueryableResolver<ApnsExternalUserDeviceToken>());

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
