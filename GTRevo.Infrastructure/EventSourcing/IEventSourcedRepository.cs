using System;
using System.Threading.Tasks;
using GTRevo.Platform.Transactions;

namespace GTRevo.Infrastructure.EventSourcing
{
    public interface IEventSourcedRepository : ITransactionProvider
    {
        void Add<T>(T aggregateRoot) where T : class, IEventSourcedAggregateRoot;

        T Get<T>(Guid id) where T : class, IEventSourcedAggregateRoot;
        IEventSourcedAggregateRoot Get(Guid id);
        Task<T> GetAsync<T>(Guid id) where T : class, IEventSourcedAggregateRoot;
        Task<IEventSourcedAggregateRoot> GetAsync(Guid id);

        /// <summary>
        /// Saves the repository changes. Not needed when using unit of work.
        /// </summary>
        void SaveChanges();

        /// <summary>
        /// Asynchronously save the repository changes. Not needed when using unit of work.
        /// </summary>
        Task SaveChangesAsync();
    }
}
