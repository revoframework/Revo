using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Revo.DataAccess.Entities;
using Revo.Infrastructure.EF6.Sagas.Model;
using Revo.Infrastructure.Sagas;

namespace Revo.Infrastructure.EF6.Sagas
{
    public class EF6SagaMetadataRepository : ISagaMetadataRepository
    {
        private readonly ICrudRepository crudRepository;

        private readonly Dictionary<Guid, List<SagaMetadataKey>> sagaMetadatas =
            new Dictionary<Guid, List<SagaMetadataKey>>();

        public EF6SagaMetadataRepository(ICrudRepository crudRepository)
        {
            this.crudRepository = crudRepository;
        }

        public Task<Guid[]> FindSagaIdsByKeyAsync(string keyName, string keyValue)
        {
            return crudRepository.Where<SagaMetadataKey>(x => x.KeyName == keyName
                                                              && x.KeyValue == keyValue)
                .Select(x => x.SagaId)
                .Distinct()
                .ToArrayAsync();
        }

        public async Task<SagaMetadata> GetSagaMetadataAsync(Guid sagaId)
        {
            List<SagaMetadataKey> keys = await GetSagaMetadataKeysAsync(sagaId);
            return new SagaMetadata(keys.GroupBy(x => x.KeyName)
                .ToImmutableDictionary(x => x.Key,
                    x => x.Select(y => y.KeyValue).ToImmutableList()));
        }

        public async Task SetSagaMetadataAsync(Guid sagaId, SagaMetadata sagaMetadata)
        {
            List<SagaMetadataKey> keys = await GetSagaMetadataKeysAsync(sagaId);

            foreach (var keyGroup in sagaMetadata.Keys)
            {
                foreach (var keyValue in keyGroup.Value)
                {
                    SagaMetadataKey key = keys.FirstOrDefault(x => x.KeyName == keyGroup.Key && x.KeyValue == keyValue);
                    if (key != null)
                    {
                        keys.Remove(key);
                        continue;
                    }

                    key = new SagaMetadataKey()
                    {
                        Id = Guid.NewGuid(),
                        KeyName = keyGroup.Key,
                        KeyValue = keyValue,
                        SagaId = sagaId
                    };
                    
                    crudRepository.Add(key);
                }
            }

            foreach (SagaMetadataKey key in keys) //remove unmatched
            {
                crudRepository.Remove(key);
            }
        }

        public void SaveChanges()
        {
            crudRepository.SaveChanges();
        }

        public Task SaveChangesAsync()
        {
            return crudRepository.SaveChangesAsync();
        }

        private async Task<List<SagaMetadataKey>> GetSagaMetadataKeysAsync(Guid sagaId)
        {
            if (!sagaMetadatas.TryGetValue(sagaId, out List<SagaMetadataKey> keys))
            {
                keys = await crudRepository.Where<SagaMetadataKey>(x => x.SagaId == sagaId)
                    .ToListAsync();
                sagaMetadatas[sagaId] = keys;
            }

            return keys;
        }
    }
}
