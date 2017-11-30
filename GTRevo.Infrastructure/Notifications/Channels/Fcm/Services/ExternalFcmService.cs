using System.Threading.Tasks;
using System.Web.Http;
using GTRevo.Infrastructure.Notifications.Channels.Fcm.Commands;
using GTRevo.Platform.Web;

namespace GTRevo.Infrastructure.Notifications.Channels.Fcm.Services
{
    public class ExternalFcmService : CommandApiController
    {
        [AcceptVerbs("POST")]
        public Task RegisterDevice(RegisterFcmExternalUserDeviceCommand parameters)
        {
            return CommandBus.Send(parameters);
        }

        [AcceptVerbs("POST")]
        public Task DeregisterDevice(DeregisterFcmExternalUserDeviceCommand parameters)
        {
            return CommandBus.Send(parameters);
        }

        [AcceptVerbs("POST")]
        public Task PushNotification(PushExternalFcmNotificationCommand parameters)
        {
            return CommandBus.Send(parameters);
        }
    }
}
