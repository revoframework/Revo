using System;
using System.Collections.Generic;
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
                .ToArrayAsync();
        }

        public async Task<SagaMetadata> GetSagaMetadataAsync(Guid sagaId)
        {
            List<SagaMetadataKey> keys = await GetSagaMetadataKeysAsync(sagaId);
            return new SagaMetadata(keys.Select(x => new KeyValuePair<string, string>(x.KeyName, x.KeyValue)));
        }

        public async Task SetSagaMetadataAsync(Guid sagaId, SagaMetadata sagaMetadata)
        {
            List<SagaMetadataKey> keys = await GetSagaMetadataKeysAsync(sagaId);

            foreach (var keyPair in sagaMetadata.Keys)
            {
                SagaMetadataKey key = keys.FirstOrDefault(x => x.KeyName == keyPair.Key);
                if (key != null)
                {
                    if (key.KeyValue != keyPair.Value)
                    {
                        key.KeyValue = keyPair.Value;
                    }
                }
                else
                {
                    key = new SagaMetadataKey()
                    {
                        KeyName = keyPair.Key,
                        KeyValue = keyPair.Value,
                        SagaId = sagaId
                    };

                    crudRepository.Add(key);
                }
            }

            foreach (var unmatched in keys.Where(x => !sagaMetadata.Keys.ContainsKey(x.KeyName)).ToList())
            {
                keys.Remove(unmatched);
                crudRepository.Remove(unmatched);
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
