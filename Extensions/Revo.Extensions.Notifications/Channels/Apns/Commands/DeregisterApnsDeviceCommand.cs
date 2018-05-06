using System.ComponentModel.DataAnnotations;
using Revo.Core.Commands;
using Revo.Infrastructure.Security.Commands;

namespace Revo.Extensions.Notifications.Channels.Apns.Commands
{
    [AuthorizePermissions(Permissions.DeregisterDeviceToken)]
    public class DeregisterApnsDeviceCommand : ICommand
    {
        [Required]
        public string DeviceToken { get; set; }

        [Required]
        public string AppId { get; set; }
    }
}
