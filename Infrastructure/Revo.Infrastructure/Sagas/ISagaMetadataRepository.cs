using System;
using System.Threading.Tasks;

namespace Revo.Infrastructure.Sagas
{
    public interface ISagaMetadataRepository
    {
        Task<Guid[]> FindSagaIdsByKeyAsync(string keyName, string keyValue);
        Task<SagaMetadata> GetSagaMetadataAsync(Guid sagaId);
        Task SetSagaMetadataAsync(Guid sagaId, SagaMetadata sagaMetadata);
        void SaveChanges();
        Task SaveChangesAsync();
    }
}
