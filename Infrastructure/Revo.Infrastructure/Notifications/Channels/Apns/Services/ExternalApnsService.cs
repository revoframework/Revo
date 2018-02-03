using System.Threading.Tasks;
using System.Web.Http;
using Revo.Infrastructure.Notifications.Channels.Apns.Commands;
using Revo.Platforms.AspNet.Web;

namespace Revo.Infrastructure.Notifications.Channels.Apns.Services
{
    public class ExternalApnsService : CommandApiController
    {
        [AcceptVerbs("POST")]
        public Task RegisterDevice(RegisterApnsExternalUserDeviceCommand parameters)
        {
            return CommandBus.SendAsync(parameters);
        }

        [AcceptVerbs("POST")]
        public Task DeregisterDevice(DeregisterApnsExternalUserDeviceCommand parameters)
        {
            return CommandBus.SendAsync(parameters);
        }

        [AcceptVerbs("POST")]
        public Task PushNotification(PushExternalApnsNotificationCommand parameters)
        {
            return CommandBus.SendAsync(parameters);
        }
    }
}
