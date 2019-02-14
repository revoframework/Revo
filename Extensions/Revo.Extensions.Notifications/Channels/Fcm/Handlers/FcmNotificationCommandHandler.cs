using System;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using Revo.Core.Commands;
using Revo.Core.Security;
using Revo.Extensions.Notifications.Channels.Fcm.Commands;
using Revo.Extensions.Notifications.Channels.Fcm.Model;
using Revo.Infrastructure.Repositories;

namespace Revo.Extensions.Notifications.Channels.Fcm.Handlers
{
    public class FcmNotificationCommandHandler :
        ICommandHandler<RegisterFcmDeviceCommand>,
        ICommandHandler<DeregisterFcmDeviceCommand>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IRepository repository;
        private readonly IUserContext userContext;

        public FcmNotificationCommandHandler(IRepository repository, IUserContext userContext)
        {
            this.repository = repository;
            this.userContext = userContext;
        }

        public async Task HandleAsync(RegisterFcmDeviceCommand message, CancellationToken cancellationToken)
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
            
            Logger.Debug($"Added external FCM user device registaration for user ID {userContext.UserId}");
        }

        public async Task HandleAsync(DeregisterFcmDeviceCommand message, CancellationToken cancellationToken)
        {
            string normalizedDeviceToken = message.RegistrationId;
            FcmUserDeviceToken token = await repository.FirstOrDefaultAsync<FcmUserDeviceToken>(
                x => x.RegistrationId == normalizedDeviceToken
                     && x.AppId == message.AppId);

            if (token != null)
            {
                repository.Remove(token);
            }
        }
    }
}
