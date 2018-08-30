using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Revo.Infrastructure.Sagas
{
    public interface ISagaMetadataRepository
    {
        void AddSaga(Guid id, Guid classId);
        Task<SagaMatch[]> FindSagasAsync(Guid classId);
        Task<SagaMatch[]> FindSagasByKeyAsync(Guid classId, string keyName, string keyValue);
        Task<SagaMetadata> GetSagaMetadataAsync(Guid sagaId);
        Task SetSagaKeysAsync(Guid sagaId, IEnumerable<KeyValuePair<string, string>> keys);
        Task SaveChangesAsync();
    }
}
