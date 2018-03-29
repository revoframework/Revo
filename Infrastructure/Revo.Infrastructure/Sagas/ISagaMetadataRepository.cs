using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Revo.Infrastructure.Sagas
{
    public interface ISagaMetadataRepository
    {
        void AddSaga(Guid sagaId, Guid sagaClassId);
        Task<SagaKeyMatch[]> FindSagasByKeyAsync(string keyName, string keyValue);
        Task<SagaMetadata> GetSagaMetadataAsync(Guid sagaId);
        Task SetSagaKeysAsync(Guid sagaId, IEnumerable<KeyValuePair<string, string>> keys);
        void SaveChanges();
        Task SaveChangesAsync();
    }
}
