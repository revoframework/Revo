using System.Threading.Tasks;
using System.Web.Http;
using Revo.AspNet.Web;
using Revo.Extensions.Notifications.Channels.Apns.Commands;

namespace Revo.Extensions.AspNet.Interop.Notifications.Apns
{
    [RoutePrefix("api/apns-device-registration-service")]
    public class ApnsDeviceRegistrationService : CommandApiController
    {
        [AcceptVerbs("POST")]
        [Route("register-device")]
        public Task RegisterDevice(RegisterApnsDeviceCommand parameters)
        {
            return CommandBus.SendAsync(parameters);
        }

        [AcceptVerbs("POST")]
        [Route("deregister-device")]
        public Task DeregisterDevice(DeregisterApnsDeviceCommand parameters)
        {
            return CommandBus.SendAsync(parameters);
        }
    }
}
