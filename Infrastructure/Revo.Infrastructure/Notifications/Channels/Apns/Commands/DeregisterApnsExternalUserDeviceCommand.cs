using System.ComponentModel.DataAnnotations;
using Revo.Core.Commands;
using Revo.Infrastructure.Security.Commands;

namespace Revo.Infrastructure.Notifications.Channels.Apns.Commands
{
    [AuthorizePermissions(Permissions.DeregisterExternalUserDeviceToken)]
    public class DeregisterApnsExternalUserDeviceCommand : ICommand
    {
        [Required]
        public string DeviceToken { get; set; }

        [Required]
        public string AppId { get; set; }
    }
}
