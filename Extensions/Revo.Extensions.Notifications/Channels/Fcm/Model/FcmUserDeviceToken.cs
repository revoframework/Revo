using System;
using Revo.Core.Core;
using Revo.Core.Security;
using Revo.DataAccess.Entities;
using Revo.Domain.Entities.Basic;

namespace Revo.Extensions.Notifications.Channels.Fcm.Model
{
    [TablePrefix(NamespacePrefix = "REV", ColumnPrefix = "FUT")]
    public class FcmUserDeviceToken : BasicAggregateRoot
    {
        public FcmUserDeviceToken(Guid id, IUser user, string registrationId, string appId) : base(id)
        {
            if (string.IsNullOrEmpty(registrationId))
            {
                throw new ArgumentException($"Invalid FCM device registration ID: '{registrationId}'");
            }
            
            UserId = user.Id;
            RegistrationId = registrationId;
            AppId = appId;
            IssuedDateTime = Clock.Current.Now;
        }

        protected FcmUserDeviceToken()
        {
        }

        public Guid UserId { get; private set; }
        public string RegistrationId { get; private set; }
        public string AppId { get; private set; }
        public DateTimeOffset IssuedDateTime { get; private set; }
    }
}
