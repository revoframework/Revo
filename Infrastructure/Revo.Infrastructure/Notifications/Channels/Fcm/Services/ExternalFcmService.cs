using System.Threading.Tasks;
using System.Web.Http;
using Revo.Infrastructure.Notifications.Channels.Fcm.Commands;
using Revo.Platforms.AspNet.Web;

namespace Revo.Infrastructure.Notifications.Channels.Fcm.Services
{
    public class ExternalFcmService : CommandApiController
    {
        [AcceptVerbs("POST")]
        public Task RegisterDevice(RegisterFcmExternalUserDeviceCommand parameters)
        {
            return CommandBus.SendAsync(parameters);
        }

        [AcceptVerbs("POST")]
        public Task DeregisterDevice(DeregisterFcmExternalUserDeviceCommand parameters)
        {
            return CommandBus.SendAsync(parameters);
        }

        [AcceptVerbs("POST")]
        public Task PushNotification(PushExternalFcmNotificationCommand parameters)
        {
            return CommandBus.SendAsync(parameters);
        }
    }
}
