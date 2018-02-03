using System;
using System.ComponentModel.DataAnnotations;
using Revo.Core.Commands;
using Revo.Infrastructure.Security.Commands;

namespace Revo.Infrastructure.Notifications.Channels.Apns.Commands
{
    [AuthorizePermissions(Permissions.RegisterExternalUserDeviceToken)]
    public class RegisterApnsExternalUserDeviceCommand : ICommand
    {
        [Required]
        public string DeviceToken { get; set; }

        [Required]
        public string AppId { get; set; }

        /// <summary>
        /// ID of user to register this device token with.
        /// </summary>
        public Guid UserId { get; set; }
    }
}
