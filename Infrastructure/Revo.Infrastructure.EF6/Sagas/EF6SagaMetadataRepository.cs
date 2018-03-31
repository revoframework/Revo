using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Revo.DataAccess.EF6.Entities;
using Revo.DataAccess.Entities;
using Revo.Infrastructure.EF6.Sagas.Model;
using Revo.Infrastructure.Sagas;
using EntityState = Revo.DataAccess.Entities.EntityState;

namespace Revo.Infrastructure.EF6.Sagas
{
    public class EF6SagaMetadataRepository : ISagaMetadataRepository
    {
        private readonly IEF6CrudRepository crudRepository;
        private readonly Dictionary<Guid, SagaMetadataRecord> metadataRecords = new Dictionary<Guid, SagaMetadataRecord>();

        public EF6SagaMetadataRepository(IEF6CrudRepository crudRepository)
        {
            this.crudRepository = crudRepository;
        }

        public void AddSaga(Guid sagaId, Guid sagaClassId)
        {
            if (metadataRecords.ContainsKey(sagaId))
            {
                throw new ArgumentException($"Saga with ID {sagaId} already has metadata added");
            }

            var metadataRecord = metadataRecords[sagaId] = new SagaMetadataRecord(sagaId, sagaClassId);
            crudRepository.Add(metadataRecord);
        }

        public async Task<SagaKeyMatch[]> FindSagasByKeyAsync(string keyName, string keyValue)
        {
            var databaseKeys = (await crudRepository.FindAll<SagaMetadataRecord>()
                .Where(x => x.Keys.Any(y => y.KeyName == keyName && y.KeyValue == keyValue))
                .ToArrayAsync())
                .Where(x => (crudRepository.GetEntityState(x) & EntityState.Deleted) == 0);

            var withCachedKeys = databaseKeys
                .Concat(metadataRecords.Values) // merge with keys in cache they might've gotten updated in the meantime
                .Distinct()
                .Where(x => x.Keys.Any(y => y.KeyName == keyName && y.KeyValue == keyValue));

            var metadata = withCachedKeys
                .Select(x => new SagaKeyMatch() {Id = x.Id, ClassId = x.ClassId})
                .ToArray();
            return metadata;
        }

        public async Task<SagaMetadata> GetSagaMetadataAsync(Guid sagaId)
        {
            var metadataRecord = await GetMetadataRecordAsync(sagaId);

            var keys = metadataRecord.Keys.GroupBy(x => x.KeyName)
                .Select(x =>
                    new KeyValuePair<string, ImmutableList<string>>(x.Key,
                        x.Select(y => y.KeyValue).ToImmutableList()));
            return new SagaMetadata(keys.ToImmutableDictionary(), metadataRecord.ClassId);
        }

        public async Task SetSagaKeysAsync(Guid sagaId, IEnumerable<KeyValuePair<string, string>> keys)
        {
            var metadataRecord = await GetMetadataRecordAsync(sagaId);
            var unmatchedKeys = metadataRecord.Keys.ToList();

            foreach (var keyPair in keys)
            {
                SagaMetadataKey key = unmatchedKeys.FirstOrDefault(x => x.KeyName == keyPair.Key && x.KeyValue == keyPair.Value);
                if (key != null)
                {
                    unmatchedKeys.Remove(key);
                    continue;
                }

                key = new SagaMetadataKey(Guid.NewGuid(), sagaId, keyPair.Key, keyPair.Value);
                metadataRecord.Keys.Add(key);
                crudRepository.Add(key);
            }

            foreach (SagaMetadataKey key in unmatchedKeys)
            {
                metadataRecord.Keys.Remove(key);
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

        private async Task<SagaMetadataRecord> GetMetadataRecordAsync(Guid sagaId)
        {
            if (!metadataRecords.TryGetValue(sagaId, out SagaMetadataRecord metadataRecord))
            {
                metadataRecord = await crudRepository.Where<SagaMetadataRecord>(x => x.Id == sagaId)
                    .Include(x => x.Keys)
                    .FirstAsync();
                metadataRecords[sagaId] = metadataRecord;
            }

            return metadataRecord;
        }
    }
}
