using System.Threading.Tasks;
using System.Web.Http;
using GTRevo.Infrastructure.Notifications.Channels.Fcm.Commands;
using GTRevo.Platform.Web;

namespace GTRevo.Infrastructure.Notifications.Channels.Fcm.Services
{
    public class FcmDeviceRegistrationService : CommandApiController
    {
        [AcceptVerbs("POST")]
        public Task RegisterDevice(RegisterFcmDeviceCommand parameters)
        {
            return CommandBus.Send(parameters);
        }

        [AcceptVerbs("POST")]
        public Task DeregisterDevice(DeregisterFcmDeviceCommand parameters)
        {
            return CommandBus.Send(parameters);
        }
    }
}
