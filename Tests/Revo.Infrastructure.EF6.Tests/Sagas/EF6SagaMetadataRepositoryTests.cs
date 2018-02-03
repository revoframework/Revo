using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Revo.DataAccess.Entities;
using Revo.Infrastructure.EF6.Sagas;
using Revo.Infrastructure.EF6.Sagas.Model;
using Revo.Infrastructure.Sagas;
using Revo.Testing.DataAccess;
using Xunit;

namespace Revo.Infrastructure.EF6.Tests.Sagas
{
    public class EF6SagaMetadataRepositoryTests
    {
        private readonly EF6SagaMetadataRepository sut;
        private readonly FakeCrudRepository fakeCrudRepository;

        public EF6SagaMetadataRepositoryTests()
        {
            fakeCrudRepository = new FakeCrudRepository();
            sut = new EF6SagaMetadataRepository(fakeCrudRepository);
        }

        [Fact]
        public async Task FindSagaIdsByKeyAsync_FindsSagas()
        {
            Guid saga1Id = Guid.NewGuid();
            Guid saga2Id = Guid.NewGuid();

            fakeCrudRepository.AttachRange<SagaMetadataKey>(new List<SagaMetadataKey>()
            {
                new SagaMetadataKey() {SagaId = saga1Id, KeyName = "key", KeyValue = "value"},
                new SagaMetadataKey() {SagaId = Guid.NewGuid(), KeyName = "key2", KeyValue = "value"},
                new SagaMetadataKey() {SagaId = saga2Id, KeyName = "key", KeyValue = "value"}
            });

            Guid[] result = await sut.FindSagaIdsByKeyAsync("key", "value");

            Assert.Contains(saga1Id, result);
            Assert.Contains(saga2Id, result);
        }

        [Fact]
        public async Task SetSagaMetadataAsync_AddsNew()
        {
            Guid sagaId = Guid.NewGuid();
            await sut.SetSagaMetadataAsync(sagaId, new SagaMetadata(new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("key", "value")
            }));

            var allMetadataKeys = fakeCrudRepository.FindAllWithAdded<SagaMetadataKey>().ToList();
            Assert.Contains(allMetadataKeys, x => x.KeyName == "key" && x.KeyValue == "value" && x.SagaId == sagaId);
            Assert.Equal(1, allMetadataKeys.Count);
        }

        [Fact]
        public async Task SetSagaMetadataAsync_UpdatesExisting()
        {
            Guid sagaId = Guid.NewGuid();
            fakeCrudRepository.Attach(new SagaMetadataKey() {SagaId = sagaId, KeyName = "name", KeyValue = "value"});

            await sut.SetSagaMetadataAsync(sagaId, new SagaMetadata(new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("name", "value")
            }));

            var allMetadataKeys = fakeCrudRepository.FindAllWithAdded<SagaMetadataKey>().ToList();
            Assert.Contains(allMetadataKeys, x => x.KeyName == "name" && x.KeyValue == "value" && x.SagaId == sagaId);
            Assert.Equal(1, allMetadataKeys.Count);
        }

        [Fact]
        public async Task SetSagaMetadataAsync_RemovesUnmatched()
        {
            Guid sagaId = Guid.NewGuid();
            var key = new SagaMetadataKey() {SagaId = sagaId, KeyName = "name", KeyValue = "value"};
            fakeCrudRepository.Attach(key);

            await sut.SetSagaMetadataAsync(sagaId, new SagaMetadata(new List<KeyValuePair<string, string>>()
            {
            }));
            
            Assert.Equal(EntityState.Deleted, fakeCrudRepository.GetEntityState(key));
        }
    }
}
