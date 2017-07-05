using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GTRevo.Infrastructure.Core.Domain;

namespace GTRevo.Infrastructure.Repositories
{
    public interface IAggregateStore
    {

        void Add<T>(T aggregate) where T : class, IAggregateRoot;

        bool CanHandleAggregateType(Type aggregateType);

        T Get<T>(Guid id) where T : class, IAggregateRoot;
        Task<T> GetAsync<T>(Guid id) where T : class, IAggregateRoot;

        IEnumerable<IAggregateRoot> GetTrackedAggregates();

        void Remove<T>(T entity) where T : class, IAggregateRoot;
        
        void SaveChanges();
        Task SaveChangesAsync();
    }
}
