using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Revo.Domain.Entities;
using Revo.Domain.Sagas;
using Revo.Infrastructure.EventSourcing;

namespace Revo.Infrastructure.Sagas
{
    public interface ISagaRepository
    {
        void Add(ISaga saga);
        Task<ISaga> GetAsync(Guid id, Guid classId);
        Task<T> GetAsync<T>(Guid id) where T : class, ISaga;
        Task SaveChangesAsync();
    }
}
