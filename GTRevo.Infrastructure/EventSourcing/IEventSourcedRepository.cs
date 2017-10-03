using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GTRevo.Core.Transactions;
using GTRevo.DataAccess.Entities;
using GTRevo.Infrastructure.Core.Domain;
using GTRevo.Infrastructure.Core.Domain.EventSourcing;

namespace GTRevo.Infrastructure.EventSourcing
{
    public interface IEventSourcedRepository<TBase> : ITransactionProvider
        where TBase : class, IEventSourcedAggregateRoot
    {
        IEnumerable<IRepositoryFilter> DefaultFilters { get; }

        void Add<T>(T aggregate) where T : class, TBase;

        T Get<T>(Guid id) where T : class, TBase;
        TBase Get(Guid id);
        Task<T> GetAsync<T>(Guid id) where T : class, TBase;
        Task<TBase> GetAsync(Guid id);

        IEnumerable<TBase> GetLoadedAggregates();

        void Remove<T>(T aggregateRoot) where T : class, TBase;

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
