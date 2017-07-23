using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using GTRevo.Infrastructure.Notifications.Channels.Push.Commands;
using GTRevo.Platform.Web;

namespace GTRevo.Infrastructure.Notifications.Channels.Push.Services
{
    public class ApnsDeviceRegistrationService : CommandApiController
    {
        [AcceptVerbs("POST")]
        public Task RegisterDevice(RegisterApnsDeviceCommand parameters)
        {
            return CommandBus.Send(parameters);
        }

        [AcceptVerbs("POST")]
        public Task DeregisterDevice(DeregisterApnsDeviceCommand parameters)
        {
            return CommandBus.Send(parameters);
        }
    }
}
