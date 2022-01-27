using System;
using System.Threading.Tasks;
using Revo.DataAccess.Entities;
using Revo.Extensions.Notifications.Model;

namespace Revo.Extensions.Notifications.Channels.Buffering
{
    public class BufferedNotificationStore : IBufferedNotificationStore
    {
        private readonly ICrudRepository crudRepository;

        public BufferedNotificationStore(ICrudRepository crudRepository)
        {
            this.crudRepository = crudRepository;
        }
        
        public Task CommitAsync()
        {
            return crudRepository.SaveChangesAsync();
        }

        public async Task Add(SerializedNotification serializedNotification, string bufferName,
            DateTimeOffset timeQueued, string bufferGovernorName, string notificationPipelineName)
        {
            NotificationBuffer buffer = await crudRepository.FirstOrDefaultAsync<NotificationBuffer>(x => x.Name == bufferName);
            if (buffer == null)
            {
                buffer = new NotificationBuffer(Guid.NewGuid(), bufferName, bufferGovernorName, notificationPipelineName);
                crudRepository.Add(buffer); // TODO race condition - use AddOrUpdate instead
            }

            BufferedNotification notification = new BufferedNotification(
                id: Guid.NewGuid(),
                buffer: buffer,
                notificationClassName: serializedNotification.NotificationClassName,
                notificationJson: serializedNotification.NotificationJson,
                timeQueued: timeQueued
            );

            crudRepository.Add(notification);
        }
    }
}
