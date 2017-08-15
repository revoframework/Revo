using System;
using System.ComponentModel.DataAnnotations;
using GTRevo.Core.Commands;
using GTRevo.Infrastructure.Security.Commands;
using Newtonsoft.Json.Linq;

namespace GTRevo.Infrastructure.Notifications.Channels.Push.Commands
{
    [AuthorizePermissions(Permissions.PushExternalNotification)]
    public class PushExternalNotificationCommand : ICommand
    {
        public Guid UserId { get; set; }

        [Required]
        public string AppId { get; set; }

        [Required]
        public JObject NotificationData { get; set; }
    }
}
