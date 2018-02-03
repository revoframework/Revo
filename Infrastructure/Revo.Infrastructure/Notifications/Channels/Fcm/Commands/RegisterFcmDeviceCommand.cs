using System.ComponentModel.DataAnnotations;
using Revo.Core.Commands;
using Revo.Infrastructure.Security.Commands;

namespace Revo.Infrastructure.Notifications.Channels.Fcm.Commands
{
    [AuthorizePermissions(Permissions.RegisterDeviceToken)]
    public class RegisterFcmDeviceCommand : ICommand
    {
        [Required]
        public string RegistrationId { get; set; }

        [Required]
        public string AppId { get; set; }

        /// <summary>
        /// ID of user to register this device token with.
        /// </summary>
        //public Guid UserId { get; set; }
    }
}
