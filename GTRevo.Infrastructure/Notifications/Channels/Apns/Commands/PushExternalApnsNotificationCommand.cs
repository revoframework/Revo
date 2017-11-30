using System;
using System.ComponentModel.DataAnnotations;
using GTRevo.Core.Commands;
using GTRevo.Infrastructure.Security.Commands;
using Newtonsoft.Json.Linq;

namespace GTRevo.Infrastructure.Notifications.Channels.Apns.Commands
{
    [AuthorizePermissions(Permissions.PushExternalNotification)]
    public class PushExternalApnsNotificationCommand : ICommand
    {
        [Required]
        public Guid[] UserIds { get; set; }

        [Required]
        public string AppId { get; set; }

        [Required]
        public JObject Payload { get; set; }
    }
}
