using System;
using System.Threading.Tasks;
using GTRevo.Core.Commands;
using GTRevo.Core.Security;
using GTRevo.Infrastructure.Notifications.Channels.Fcm.Commands;
using GTRevo.Infrastructure.Notifications.Channels.Fcm.Model;
using GTRevo.Infrastructure.Repositories;
using NLog;

namespace GTRevo.Infrastructure.Notifications.Channels.Fcm.CommandHandlers
{
    public class FcmNotificationCommandHandler :
        IAsyncCommandHandler<RegisterFcmDeviceCommand>,
        IAsyncCommandHandler<DeregisterFcmDeviceCommand>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IRepository repository;
        private readonly IUserContext userContext;

        public FcmNotificationCommandHandler(IRepository repository, IUserContext userContext)
        {
            this.repository = repository;
            this.userContext = userContext;
        }

        public async Task Handle(RegisterFcmDeviceCommand message)
        {
            if (!userContext.IsAuthenticated)
            {
                throw new InvalidOperationException("Cannot register FCM device for an unauthenticated user");
            }

            string normalizedDeviceToken = message.RegistrationId;
            FcmUserDeviceToken token = await repository.FirstOrDefaultAsync<FcmUserDeviceToken>(
                x => x.RegistrationId == normalizedDeviceToken
                     && x.AppId == message.AppId);

            if (token != null)
            {
                if (token.UserId == userContext.UserId)
                {
                    return;
                }

                repository.Remove(token);
            }

            // TODO restrict AppIds (a configurable list?)

            token = new FcmUserDeviceToken(Guid.NewGuid(), await userContext.GetUserAsync(),
                normalizedDeviceToken, message.AppId);
            repository.Add(token);

            await repository.SaveChangesAsync();
            Logger.Debug($"Added external FCM user device registaration for user ID {userContext.UserId}");
        }

        public async Task Handle(DeregisterFcmDeviceCommand message)
        {
            string normalizedDeviceToken = message.RegistrationId;
            FcmUserDeviceToken token = await repository.FirstOrDefaultAsync<FcmUserDeviceToken>(
                x => x.RegistrationId == normalizedDeviceToken
                     && x.AppId == message.AppId);

            if (token != null)
            {
                repository.Remove(token);
            }

            await repository.SaveChangesAsync();
        }
    }
}
