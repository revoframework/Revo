using System.Threading.Tasks;
using System.Web.Http;
using Revo.Infrastructure.Notifications.Channels.Fcm.Commands;
using Revo.Platforms.AspNet.Web;

namespace Revo.Infrastructure.Notifications.Channels.Fcm.Services
{
    public class FcmDeviceRegistrationService : CommandApiController
    {
        [AcceptVerbs("POST")]
        public Task RegisterDevice(RegisterFcmDeviceCommand parameters)
        {
            return CommandBus.SendAsync(parameters);
        }

        [AcceptVerbs("POST")]
        public Task DeregisterDevice(DeregisterFcmDeviceCommand parameters)
        {
            return CommandBus.SendAsync(parameters);
        }
    }
}
