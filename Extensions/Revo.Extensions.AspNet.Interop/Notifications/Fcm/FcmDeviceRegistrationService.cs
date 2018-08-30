using System.Threading.Tasks;
using System.Web.Http;
using Revo.AspNet.Web;
using Revo.Extensions.Notifications.Channels.Fcm.Commands;

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
