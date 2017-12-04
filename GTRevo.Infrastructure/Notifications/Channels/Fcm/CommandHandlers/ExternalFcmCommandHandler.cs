using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using GTRevo.Core.Commands;
using GTRevo.Infrastructure.Notifications.Channels.Fcm.Commands;
using GTRevo.Infrastructure.Notifications.Channels.Fcm.Model;
using GTRevo.Infrastructure.Repositories;
using NLog;
using PushSharp.Google;

namespace GTRevo.Infrastructure.Notifications.Channels.Fcm.CommandHandlers
{
    public class ExternalFcmNotificationCommandHandler :
        IAsyncCommandHandler<RegisterFcmExternalUserDeviceCommand>,
        IAsyncCommandHandler<DeregisterFcmExternalUserDeviceCommand>,
        IAsyncCommandHandler<PushExternalFcmNotificationCommand>
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

        public async Task Handle(RegisterFcmExternalUserDeviceCommand message)
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

            await repository.SaveChangesAsync();
            Logger.Debug($"Added external APNS external user device token for user ID {message.UserId}");
        }

        public async Task Handle(DeregisterFcmExternalUserDeviceCommand message)
        {
            string normalizedDeviceToken = message.RegistrationId;
            FcmExternalUserDeviceToken token = await repository.FirstOrDefaultAsync<FcmExternalUserDeviceToken>(
                x => x.RegistrationId == normalizedDeviceToken
                     && x.AppId == message.AppId);

            if (token != null)
            {
                repository.Remove(token);
                await repository.SaveChangesAsync();
            }
        }

        public async Task Handle(PushExternalFcmNotificationCommand message)
        {
            if (message.Data == null && message.Notification == null)
            {
                throw new ArgumentException(
                    "Error sending FCM notification: either Notification or/and Data fields must not be null!");
            }

            List<FcmExternalUserDeviceToken> tokens = await repository.Where<FcmExternalUserDeviceToken>(
                    x => message.UserIds.Contains(x.ExternalUserId)
                         && x.AppId == message.AppId)
                .ToListAsync();

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
