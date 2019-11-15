using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Revo.Domain.Entities;

namespace Revo.Infrastructure.Repositories
{
    public interface IAggregateStore
    {
        bool NeedsSave { get; }

        void Add<T>(T aggregate) where T : class, IAggregateRoot;

        bool CanHandleAggregateType(Type aggregateType);

        Task<T> FindAsync<T>(Guid id) where T : class, IAggregateRoot;
        Task<T[]> FindManyAsync<T>(params Guid[] ids) where T : class, IAggregateRoot;

        Task<T> GetAsync<T>(Guid id) where T : class, IAggregateRoot;
        Task<T[]> GetManyAsync<T>(params Guid[] ids) where T : class, IAggregateRoot;

        IEnumerable<IAggregateRoot> GetTrackedAggregates();

        void Remove<T>(T entity) where T : class, IAggregateRoot;
        
        Task SaveChangesAsync();
    }
}
