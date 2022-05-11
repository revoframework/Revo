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

        public AggregatingBufferGovernor(string name, TimeSpan minTimeDelay)
        {
            Name = name;
            this.minTimeDelay = minTimeDelay;
        }

        public string Name { get; }

        public async Task<MultiValueDictionary<NotificationBuffer, BufferedNotification>> SelectNotificationsForReleaseAsync(IReadRepository readRepository)
        {
            DateTimeOffset maxDate = Clock.Current.UtcNow.Subtract(minTimeDelay);
            var notifications = readRepository.FindAll<BufferedNotification>()
                .Where(x => x.TimeQueued <= maxDate && x.Buffer.GovernorName == Name);
            var buffers = await notifications
                .Include(readRepository, x => x.Buffer.Notifications)
                .Select(x => x.Buffer)
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
