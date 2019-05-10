using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Revo.DataAccess.Entities;
using Revo.Infrastructure.Sagas.Generic.Model;
using EntityState = Revo.DataAccess.Entities.EntityState;

namespace Revo.Infrastructure.Sagas.Generic
{
    public class SagaMetadataRepository : ISagaMetadataRepository
    {
        private readonly ICrudRepository crudRepository;
        private readonly Dictionary<Guid, SagaMetadataRecord> metadataRecords = new Dictionary<Guid, SagaMetadataRecord>();

        public SagaMetadataRepository(ICrudRepository crudRepository)
        {
            this.crudRepository = crudRepository;
        }

        public void AddSaga(Guid id, Guid classId)
        {
            if (metadataRecords.ContainsKey(id))
            {
                throw new ArgumentException($"Saga with ID {id} already has metadata added");
            }

            var metadataRecord = metadataRecords[id] = new SagaMetadataRecord(id, classId);
            crudRepository.Add(metadataRecord);
        }

        public Task<SagaMatch[]> FindSagasAsync(Guid classId)
        {
            return QuerySagaRecords(x => x.ClassId == classId);
        }

        public  Task<SagaMatch[]> FindSagasByKeyAsync(Guid classId, string keyName, string keyValue)
        {
            return QuerySagaRecords(x => x.ClassId == classId
                                         && x.Keys.Any(y => y.KeyName == keyName && y.KeyValue == keyValue));

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
        
        public Task SaveChangesAsync()
        {
            return crudRepository.SaveChangesAsync();
        }

        private async Task<SagaMetadataRecord> GetMetadataRecordAsync(Guid sagaId)
        {
            if (!metadataRecords.TryGetValue(sagaId, out SagaMetadataRecord metadataRecord))
            {
                metadataRecord = await crudRepository.Where<SagaMetadataRecord>(x => x.Id == sagaId)
                    .Include(crudRepository, x => x.Keys)
                    .FirstAsync(crudRepository);
                metadataRecords[sagaId] = metadataRecord;
            }

            return metadataRecord;
        }

        private async Task<SagaMatch[]> QuerySagaRecords(Expression<Func<SagaMetadataRecord, bool>> query)
        {
            var databaseKeys = (await crudRepository.FindAll<SagaMetadataRecord>()
                    .Include(crudRepository, x => x.Keys)
                    .Where(query)
                    .ToArrayAsync(crudRepository))
                .Where(x => crudRepository.GetEntityState(x) != EntityState.Deleted);

            var withCachedKeys = databaseKeys
                .Concat(metadataRecords.Values) // merge with keys in cache they might've gotten updated in the meantime
                .Distinct()
                .Where(query.Compile());

            var metadata = withCachedKeys
                .Select(x => new SagaMatch() { Id = x.Id, ClassId = x.ClassId })
                .ToArray();
            return metadata;
        }
    }
}
