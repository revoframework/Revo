using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json.Linq;
using Revo.Core.Commands;
using Revo.Infrastructure.Security.Commands;

namespace Revo.Extensions.Notifications.Channels.Fcm.Commands
{
    [AuthorizePermissions(Permissions.PushExternalNotification)]
    public class PushExternalFcmNotificationCommand : ICommand
    {
        [Required]
        public Guid[] UserIds { get; set; }

        [Required]
        public string AppId { get; set; }
        
        public JObject Notification { get; set; }
        public JObject Data { get; set; }
    }
}
