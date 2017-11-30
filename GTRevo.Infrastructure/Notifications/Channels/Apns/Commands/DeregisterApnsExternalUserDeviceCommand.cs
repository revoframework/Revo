using System.ComponentModel.DataAnnotations;
using GTRevo.Core.Commands;
using GTRevo.Infrastructure.Security.Commands;

namespace GTRevo.Infrastructure.Notifications.Channels.Apns.Commands
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
