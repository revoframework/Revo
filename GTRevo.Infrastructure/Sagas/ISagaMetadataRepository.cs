using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTRevo.Infrastructure.Sagas
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
