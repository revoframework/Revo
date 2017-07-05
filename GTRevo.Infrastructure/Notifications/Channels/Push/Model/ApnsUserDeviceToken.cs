using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.DataAccess.Entities;
using GTRevo.Infrastructure.Core.Domain.Basic;
using GTRevo.Platform.Security;

namespace GTRevo.Infrastructure.Notifications.Channels.Push.Model
{
    [TablePrefix(NamespacePrefix = "REV", ColumnPrefix = "AUT")]
    public class ApnsUserDeviceToken : BasicAggregateRoot
    {

        public ApnsUserDeviceToken(Guid id, IUser user, string deviceToken) : base(id)
        {
            this.UserId = user.Id;
            this.DeviceToken = deviceToken;
        }

        public ApnsUserDeviceToken()
        {
        }

        public Guid UserId { get; private set; }
        public string DeviceToken { get; private set; }
    }
}
