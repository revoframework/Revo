using System.Threading.Tasks;
using System.Web.Http;
using Revo.AspNet.Web;
using Revo.Extensions.Notifications.Channels.Apns.Commands;

namespace Revo.Extensions.AspNet.Interop.Notifications.Apns
{
    [RoutePrefix("external-apns-service")]
    public class ExternalApnsService : CommandApiController
    {
        [AcceptVerbs("POST")]
        [Route("register-device")]
        public Task RegisterDevice(RegisterApnsExternalUserDeviceCommand parameters)
        {
            return CommandBus.SendAsync(parameters);
        }

        [AcceptVerbs("POST")]
        [Route("deregister-device")]
        public Task DeregisterDevice(DeregisterApnsExternalUserDeviceCommand parameters)
        {
            return CommandBus.SendAsync(parameters);
        }

        [AcceptVerbs("POST")]
        [Route("push-notification")]
        public Task PushNotification(PushExternalApnsNotificationCommand parameters)
        {
            return CommandBus.SendAsync(parameters);
        }
    }
}
