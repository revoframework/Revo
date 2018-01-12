using System;
using GTRevo.Core.Core;
using GTRevo.DataAccess.Entities;
using GTRevo.Infrastructure.Core.Domain.Basic;

namespace GTRevo.Infrastructure.Notifications.Channels.Fcm.Model
{
    [TablePrefix(NamespacePrefix = "REV", ColumnPrefix = "FET")]
    public class FcmExternalUserDeviceToken : BasicAggregateRoot
    {
        public FcmExternalUserDeviceToken(Guid id, Guid externalUserId, string registrationId, string appId) : base(id)
        {
            if (string.IsNullOrEmpty(registrationId))
            {
                throw new ArgumentException($"Invalid FCM device registration ID: '{registrationId}'");
            }

            ExternalUserId = externalUserId;
            RegistrationId = registrationId;
            AppId = appId;
            IssuedDateTime = Clock.Current.Now;
        }

        protected FcmExternalUserDeviceToken()
        {
        }

        public Guid ExternalUserId { get; private set; }
        public string RegistrationId { get; private set; }
        public string AppId { get; private set; }
        public DateTimeOffset IssuedDateTime { get; private set; }
    }
}
