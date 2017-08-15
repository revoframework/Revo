using System;
using System.Text.RegularExpressions;
using GTRevo.Core.Core;
using GTRevo.DataAccess.Entities;
using GTRevo.Infrastructure.Core.Domain.Basic;
using GTRevo.Platform.Security;

namespace GTRevo.Infrastructure.Notifications.Channels.Push.Model
{
    [TablePrefix(NamespacePrefix = "REV", ColumnPrefix = "AUT")]
    public class ApnsUserDeviceToken : BasicAggregateRoot
    {
        private static readonly Regex DeviceTokenRegex = new Regex(@"^[0-9A-F]{64,}$", RegexOptions.IgnoreCase);

        public ApnsUserDeviceToken(Guid id, IUser user, string deviceToken, string appId) : base(id)
        {
            if (!DeviceTokenRegex.Match(deviceToken).Success)
            {
                throw new ArgumentException($"Invalid APNS device token: '{deviceToken}'");
            }
            
            UserId = user.Id;
            DeviceToken = deviceToken;
            AppId = appId;
            IssuedDateTime = Clock.Current.Now;
        }

        protected ApnsUserDeviceToken()
        {
        }

        public Guid UserId { get; private set; }
        public string DeviceToken { get; private set; }
        public string AppId { get; private set; }
        public DateTime IssuedDateTime { get; private set; }
    }
}
