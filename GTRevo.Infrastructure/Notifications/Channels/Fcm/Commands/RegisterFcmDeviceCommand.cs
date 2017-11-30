using System.ComponentModel.DataAnnotations;
using GTRevo.Core.Commands;
using GTRevo.Infrastructure.Security.Commands;

namespace GTRevo.Infrastructure.Notifications.Channels.Fcm.Commands
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
