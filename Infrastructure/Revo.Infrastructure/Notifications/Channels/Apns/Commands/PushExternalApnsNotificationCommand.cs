using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json.Linq;
using Revo.Core.Commands;
using Revo.Infrastructure.Security.Commands;

namespace Revo.Infrastructure.Notifications.Channels.Apns.Commands
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
