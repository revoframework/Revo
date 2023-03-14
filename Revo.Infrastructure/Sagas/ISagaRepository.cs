using System;
using System.Threading.Tasks;
using Revo.Domain.Sagas;

namespace Revo.Infrastructure.Sagas
{
    public interface ISagaRepository
    {
        void Add(ISaga saga);
        Task<ISaga> GetAsync(Guid id, Guid classId);
        Task<T> GetAsync<T>(Guid id) where T : class, ISaga;
        Task SendSagaCommandsAsync();
    }
}
