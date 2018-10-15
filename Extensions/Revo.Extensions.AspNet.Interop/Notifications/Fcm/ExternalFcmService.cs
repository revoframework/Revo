using System.Threading.Tasks;
using System.Web.Http;
using Revo.AspNet.Web;
using Revo.Extensions.Notifications.Channels.Fcm.Commands;

namespace Revo.Extensions.AspNet.Interop.Notifications.Fcm
{

    [RoutePrefix("external-fcm-service")]
    public class ExternalFcmService : CommandApiController
    {
        [AcceptVerbs("POST")]
        [Route("register-device")]
        public Task RegisterDevice(RegisterFcmExternalUserDeviceCommand parameters)
        {
            return CommandBus.SendAsync(parameters);
        }

        [AcceptVerbs("POST")]
        [Route("deregister-device")]
        public Task DeregisterDevice(DeregisterFcmExternalUserDeviceCommand parameters)
        {
            return CommandBus.SendAsync(parameters);
        }

        [AcceptVerbs("POST")]
        [Route("push-notification")]
        public Task PushNotification(PushExternalFcmNotificationCommand parameters)
        {
            return CommandBus.SendAsync(parameters);
        }
    }
}
