using System;
using System.ComponentModel.DataAnnotations;
using GTRevo.Core.Commands;
using GTRevo.Infrastructure.Security.Commands;
using Newtonsoft.Json.Linq;

namespace GTRevo.Infrastructure.Notifications.Channels.Fcm.Commands
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
