using System;
using System.Threading.Tasks;
using GTRevo.Core.Events;
using GTRevo.Core.Transactions;
using GTRevo.DataAccess.Entities;
using GTRevo.Infrastructure.Notifications.Model;

namespace GTRevo.Infrastructure.Notifications
{
    public class BufferedNotificationStore : IBufferedNotificationStore, IEventQueueTransactionListener
    {
        private readonly ICrudRepository crudRepository;

        public BufferedNotificationStore(ICrudRepository crudRepository)
        {
            this.crudRepository = crudRepository;
        }

        public void OnTransactionBegin(ITransaction transaction)
        {
        }

        public Task OnTransactionSucceededAsync(ITransaction transaction)
        {
                return crudRepository.SaveChangesAsync();
        }

        public async Task Add(SerializedNotification serializedNotification, Guid bufferId,
            DateTime timeQueued, Guid bufferGovernorId, Guid notificationPipelineId)
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
