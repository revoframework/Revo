using System;
using System.Threading.Tasks;
using GTRevo.Core.Commands;
using GTRevo.Core.Security;
using GTRevo.Infrastructure.Notifications.Channels.Apns.Commands;
using GTRevo.Infrastructure.Notifications.Channels.Apns.Model;
using GTRevo.Infrastructure.Repositories;
using GTRevo.Platform.Security;
using NLog;

namespace GTRevo.Infrastructure.Notifications.Channels.Apns.CommandHandlers
{
    public class ApnsNotificationCommandHandler :
        IAsyncCommandHandler<RegisterApnsDeviceCommand>,
        IAsyncCommandHandler<DeregisterApnsDeviceCommand>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IRepository repository;
        private readonly IUserContext userContext;

        public ApnsNotificationCommandHandler(IRepository repository, IUserContext userContext)
        {
            this.repository = repository;
            this.userContext = userContext;
        }

        public async Task Handle(RegisterApnsDeviceCommand message)
        {
            if (!userContext.IsAuthenticated)
            {
                throw new InvalidOperationException("Cannot register APNS device token for an unauthenticated user");
            }

            string normalizedDeviceToken = message.DeviceToken.Replace(" ", "");
            ApnsUserDeviceToken token = await repository.FirstOrDefaultAsync<ApnsUserDeviceToken>(
                x => x.DeviceToken == normalizedDeviceToken
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

            token = new ApnsUserDeviceToken(Guid.NewGuid(), await userContext.GetUserAsync(),
                normalizedDeviceToken, message.AppId);
            repository.Add(token);

            await repository.SaveChangesAsync();
            Logger.Debug($"Added external APNS user device token for user ID {userContext.UserId}");
        }

        public async Task Handle(DeregisterApnsDeviceCommand message)
        {
            string normalizedDeviceToken = message.DeviceToken.Replace(" ", "");
            ApnsUserDeviceToken token = await repository.FirstOrDefaultAsync<ApnsUserDeviceToken>(
                x => x.DeviceToken == normalizedDeviceToken
                     && x.AppId == message.AppId);

            if (token != null)
            {
                repository.Remove(token);
            }

            await repository.SaveChangesAsync();
        }
    }
}
