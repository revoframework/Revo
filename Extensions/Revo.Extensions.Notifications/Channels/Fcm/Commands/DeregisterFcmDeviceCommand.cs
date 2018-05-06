using System.ComponentModel.DataAnnotations;
using Revo.Core.Commands;
using Revo.Infrastructure.Security.Commands;

namespace Revo.Extensions.Notifications.Channels.Fcm.Commands
{
    [AuthorizePermissions(Permissions.DeregisterDeviceToken)]
    public class DeregisterFcmDeviceCommand : ICommand
    {
        [Required]
        public string RegistrationId { get; set; }

        [Required]
        public string AppId { get; set; }
    }
}
