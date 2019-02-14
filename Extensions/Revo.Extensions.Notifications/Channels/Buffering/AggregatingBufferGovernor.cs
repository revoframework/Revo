using System;
using System.Linq;
using System.Threading.Tasks;
using Revo.Core.Collections;
using Revo.Core.Core;
using Revo.DataAccess.Entities;
using Revo.Extensions.Notifications.Model;

namespace Revo.Extensions.Notifications.Channels.Buffering
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
            DateTimeOffset maxDate = Clock.Current.Now.Subtract(minTimeDelay);
            var notifications = readRepository.FindAll<BufferedNotification>()
                .Where(x => x.TimeQueued <= maxDate && x.Buffer.GovernorId == Id);
            var buffers = await notifications.Select(x => x.Buffer)
                .Include(readRepository, x => x.Notifications)
                .ToListAsync(readRepository);

            var res = new MultiValueDictionary<NotificationBuffer, BufferedNotification>();
            foreach (var buffer in buffers.Distinct())
            {
                res.AddRange(buffer, buffer.Notifications);
            }

            return res;
        }
    }
}
