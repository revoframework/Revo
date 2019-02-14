using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using PushSharp.Google;
using Revo.Core.Commands;
using Revo.DataAccess.Entities;
using Revo.Extensions.Notifications.Channels.Fcm.Commands;
using Revo.Extensions.Notifications.Channels.Fcm.Model;
using Revo.Infrastructure.Repositories;

namespace Revo.Extensions.Notifications.Channels.Fcm.Handlers
{
    public class ExternalFcmNotificationCommandHandler :
        ICommandHandler<RegisterFcmExternalUserDeviceCommand>,
        ICommandHandler<DeregisterFcmExternalUserDeviceCommand>,
        ICommandHandler<PushExternalFcmNotificationCommand>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IRepository repository;
        private readonly IFcmBrokerDispatcher fcmBrokerDispatcher;

        public ExternalFcmNotificationCommandHandler(IRepository repository,
            IFcmBrokerDispatcher fcmBrokerDispatcher)
        {
            this.repository = repository;
            this.fcmBrokerDispatcher = fcmBrokerDispatcher;
        }
        
        public async Task HandleAsync(RegisterFcmExternalUserDeviceCommand message, CancellationToken cancellationToken)
        {
            string normalizedDeviceToken = message.RegistrationId;
            FcmExternalUserDeviceToken token = await repository.FirstOrDefaultAsync<FcmExternalUserDeviceToken>(
                x => x.RegistrationId == normalizedDeviceToken
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

            token = new FcmExternalUserDeviceToken(Guid.NewGuid(), message.UserId,
                normalizedDeviceToken.Replace(" ", ""), message.AppId);
            repository.Add(token);
            
            Logger.Debug($"Added external APNS external user device token for user ID {message.UserId}");
        }

        public async Task HandleAsync(DeregisterFcmExternalUserDeviceCommand message, CancellationToken cancellationToken)
        {
            string normalizedDeviceToken = message.RegistrationId;
            FcmExternalUserDeviceToken token = await repository.FirstOrDefaultAsync<FcmExternalUserDeviceToken>(
                x => x.RegistrationId == normalizedDeviceToken
                     && x.AppId == message.AppId);

            if (token != null)
            {
                repository.Remove(token);
            }
        }

        public async Task HandleAsync(PushExternalFcmNotificationCommand message, CancellationToken cancellationToken)
        {
            if (message.Data == null && message.Notification == null)
            {
                throw new ArgumentException(
                    "Error sending FCM notification: either Notification or/and Data fields must not be null!");
            }

            List<FcmExternalUserDeviceToken> tokens = await repository.Where<FcmExternalUserDeviceToken>(
                    x => message.UserIds.Contains(x.ExternalUserId)
                         && x.AppId == message.AppId)
                .ToListAsync(repository.GetQueryableResolver<FcmExternalUserDeviceToken>());

            if (tokens.Count == 0)
            {
                return;
            }

            fcmBrokerDispatcher.QueueNotifications(
                tokens.Select(
                    x => new WrappedFcmNotification(new GcmNotification()
                    {
                        RegistrationIds = new List<string>() { x.RegistrationId },
                        Data = message.Data,
                        Notification = message.Notification
                    }, x.AppId)));
        }
    }
}
