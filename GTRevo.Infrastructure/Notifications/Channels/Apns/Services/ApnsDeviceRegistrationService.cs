using System.Threading.Tasks;
using System.Web.Http;
using GTRevo.Infrastructure.Notifications.Channels.Apns.Commands;
using GTRevo.Platform.Web;

namespace GTRevo.Infrastructure.Notifications.Channels.Apns.Services
{
    public class ApnsDeviceRegistrationService : CommandApiController
    {
        [AcceptVerbs("POST")]
        public Task RegisterDevice(RegisterApnsDeviceCommand parameters)
        {
            return CommandBus.SendAsync(parameters);
        }

        [AcceptVerbs("POST")]
        public Task DeregisterDevice(DeregisterApnsDeviceCommand parameters)
        {
            return CommandBus.SendAsync(parameters);
        }
    }
}
