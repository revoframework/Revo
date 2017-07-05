using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using GTRevo.Core;
using GTRevo.Core.Core;
using GTRevo.DataAccess.EF6.Entities;
using GTRevo.Infrastructure.Notifications.Model;

namespace GTRevo.Infrastructure.Notifications
{
    public class AggregatingBufferGovernor : IBufferGovernor
    {
        private readonly TimeSpan minTimeDelay;

        public AggregatingBufferGovernor(Guid id, TimeSpan minTimeDelay)
        {
            Id = id;
            this.minTimeDelay = minTimeDelay;
        }

        public Guid Id { get; }

        public async Task<MultiValueDictionary<NotificationBuffer, BufferedNotification>> SelectNotificationsForReleaseAsync(IReadRepository readRepository)
        {
            DateTime maxDate = Clock.Current.Now.Subtract(minTimeDelay);
            var buffers = readRepository.FindAll<NotificationBuffer>()
                .Where(x => x.GovernorId == Id);
            var buffersWithNotifications = buffers.GroupJoin(readRepository.FindAll<BufferedNotification>(),
                notification => notification.Id,
                buffer => buffer.Buffer.Id,
                (buffer, notifications) => new {Buffer = buffer, Notifications = notifications});
            var toRelease = await buffersWithNotifications
                .Where(x => x.Notifications.Any(y => y.TimeQueued <= maxDate))
                .ToListAsync();

            var res = new MultiValueDictionary<NotificationBuffer, BufferedNotification>();
            foreach (var pair in toRelease)
            {
                res.AddRange(pair.Buffer, pair.Notifications);
            }

            return res;
        }
    }
}
