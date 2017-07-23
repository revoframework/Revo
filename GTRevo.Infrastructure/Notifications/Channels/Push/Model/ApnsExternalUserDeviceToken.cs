using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GTRevo.Core.Core;
using GTRevo.DataAccess.Entities;
using GTRevo.Infrastructure.Core.Domain.Basic;
using GTRevo.Platform.Security;

namespace GTRevo.Infrastructure.Notifications.Channels.Push.Model
{
    [TablePrefix(NamespacePrefix = "REV", ColumnPrefix = "AET")]
    public class ApnsExternalUserDeviceToken : BasicAggregateRoot
    {
        private static readonly Regex DeviceTokenRegex = new Regex(@"^[0-9A-F]{64,}$", RegexOptions.IgnoreCase);

        public ApnsExternalUserDeviceToken(Guid id, Guid externalUserId, string deviceToken, string appId) : base(id)
        {
            if (!DeviceTokenRegex.Match(deviceToken).Success)
            {
                throw new ArgumentException($"Invalid APNS device token: '{deviceToken}'");
            }

            ExternalUserId = externalUserId;
            DeviceToken = deviceToken;
            AppId = appId;
            IssuedDateTime = Clock.Current.Now;
        }

        protected ApnsExternalUserDeviceToken()
        {
        }

        public Guid ExternalUserId { get; private set; }
        public string DeviceToken { get; private set; }
        public string AppId { get; private set; }
        public DateTime IssuedDateTime { get; private set; }
    }
}
