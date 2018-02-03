using System;
using System.Text.RegularExpressions;
using Revo.Core.Core;
using Revo.Core.Security;
using Revo.DataAccess.Entities;
using Revo.Domain.Entities.Basic;

namespace Revo.Infrastructure.Notifications.Channels.Apns.Model
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
        public DateTimeOffset IssuedDateTime { get; private set; }
    }
}
