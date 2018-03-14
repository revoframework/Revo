using System.Threading.Tasks;
using System.Web.Http;
using Revo.Infrastructure.Notifications.Channels.Fcm.Commands;
using Revo.Platforms.AspNet.Web;

namespace Revo.Extensions.AspNet.Interop.Notifications.Fcm
{
    [RoutePrefix("api/fcm-device-registration-service")]
    public class FcmDeviceRegistrationService : CommandApiController
    {
        [AcceptVerbs("POST")]
        [Route("register-device")]
        public Task RegisterDevice(RegisterFcmDeviceCommand parameters)
        {
            return CommandBus.SendAsync(parameters);
        }

        [AcceptVerbs("POST")]
        [Route("deregister-device")]
        public Task DeregisterDevice(DeregisterFcmDeviceCommand parameters)
        {
            return CommandBus.SendAsync(parameters);
        }
    }
}
