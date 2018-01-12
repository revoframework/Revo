using System;
using System.Threading.Tasks;
using GTRevo.Core.Events;
using GTRevo.Core.Transactions;
using GTRevo.DataAccess.Entities;
using GTRevo.Infrastructure.Notifications.Model;

namespace GTRevo.Infrastructure.Notifications
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

        public async Task Add(SerializedNotification serializedNotification, Guid bufferId,
            DateTimeOffset timeQueued, Guid bufferGovernorId, Guid notificationPipelineId)
        {
            NotificationBuffer buffer = await crudRepository.FindAsync<NotificationBuffer>(bufferId);
            if (buffer == null)
            {
                buffer = new NotificationBuffer(bufferId, bufferGovernorId, notificationPipelineId);
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
