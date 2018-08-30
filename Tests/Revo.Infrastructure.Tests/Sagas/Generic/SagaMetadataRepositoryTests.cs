using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Revo.DataAccess.Entities;
using Revo.DataAccess.InMemory;
using Revo.Infrastructure.Sagas;
using Revo.Infrastructure.Sagas.Generic;
using Revo.Infrastructure.Sagas.Generic.Model;
using Xunit;

namespace Revo.Infrastructure.Tests.Sagas.Generic
{
    public class SagaMetadataRepositoryTests
    {
        private readonly SagaMetadataRepository sut;
        private readonly InMemoryCrudRepository inMemoryCrudRepository;

        public SagaMetadataRepositoryTests()
        {
            inMemoryCrudRepository = new InMemoryCrudRepository();
            sut = new SagaMetadataRepository(inMemoryCrudRepository);
        }

        [Fact]
        public void AddSaga_CreatesNewSagaRecord()
        {
            Guid sagaId = Guid.Parse("1367F325-CF6C-4EB3-8C23-696A7135CD9A");
            Guid sagaClassId = Guid.Parse("10452E7E-4888-4EF0-BC78-D76C4D8062B5");
            sut.AddSaga(sagaId, sagaClassId);

            var metadatas = inMemoryCrudRepository.FindAllWithAdded<SagaMetadataRecord>().ToList();

            metadatas.Count.Should().Be(1);
            metadatas.Should().Contain(x => x.Id == sagaId && x.ClassId == sagaClassId);
        }

        [Fact]
        public async Task FindSagasAsync_ByClassId()
        {
            Guid saga1Id = Guid.Parse("6B3577DF-B0F7-444A-8506-029591BC9894");
            Guid saga2Id = Guid.Parse("75E24A6E-29BF-4863-AB6A-299EB107947D");
            Guid saga1ClassId = Guid.Parse("D3F5C4A9-DC44-49F7-B629-230852265B87");
            Guid saga2ClassId = Guid.Parse("B68CFFB0-5ECD-424B-B837-BCC4394F5465");

            var sagaMetadata1 = new SagaMetadataRecord(saga1Id, saga1ClassId);
            var sagaMetadata2 = new SagaMetadataRecord(saga2Id, saga2ClassId);

            inMemoryCrudRepository.AttachRange(new[]
            {
                sagaMetadata1, sagaMetadata2
            });

            SagaMatch[] result = await sut.FindSagasAsync(saga1ClassId);

            result.Should().BeEquivalentTo(new[]
            {
                new SagaMatch() { Id = sagaMetadata1.Id, ClassId = sagaMetadata1.ClassId }
            });
        }

        [Fact]
        public async Task FindSagasByKeyAsync_WithCorrespondingKeys()
        {
            Guid saga1Id = Guid.Parse("6B3577DF-B0F7-444A-8506-029591BC9894");
            Guid saga2Id = Guid.Parse("75E24A6E-29BF-4863-AB6A-299EB107947D");
            Guid sagaClassId = Guid.Parse("D3F5C4A9-DC44-49F7-B629-230852265B87");

            var sagaMetadata1 = new SagaMetadataRecord(saga1Id, sagaClassId);
            sagaMetadata1.Keys.AddRange(new[]
            {
                new SagaMetadataKey(Guid.Parse("FC9083F9-0FB1-46DE-BC50-EECD5D5B1A79"), saga1Id, "key", "value")
            });

            var sagaMetadata2 = new SagaMetadataRecord(saga2Id, sagaClassId);
            sagaMetadata2.Keys.AddRange(new[]
            {
                new SagaMetadataKey(Guid.Parse("919274C2-E501-430D-A60D-60DFD486C0EB"), saga2Id, "key", "value")
            });

            var sagaMetadata3 = new SagaMetadataRecord(Guid.NewGuid(), sagaClassId);
            sagaMetadata3.Keys.AddRange(new[]
            {
                new SagaMetadataKey(Guid.NewGuid(), sagaMetadata3.Id, "key2", "value")
            });

            inMemoryCrudRepository.AttachRange(sagaMetadata1.Keys.Concat(sagaMetadata2.Keys).Concat(sagaMetadata3.Keys));
            inMemoryCrudRepository.AttachRange(new[]
            {
                sagaMetadata1, sagaMetadata2, sagaMetadata3
            });

            SagaMatch[] result = await sut.FindSagasByKeyAsync(sagaClassId, "key", "value");

            result.Should().BeEquivalentTo(new[]
            {
                new SagaMatch() { Id = sagaMetadata1.Id, ClassId = sagaMetadata1.ClassId },
                new SagaMatch() { Id = sagaMetadata2.Id, ClassId = sagaMetadata2.ClassId }
            });
        }

        [Fact]
        public async Task FindSagasByKeyAsync_OnlyWithSpecifiedClassId()
        {
            Guid saga1Id = Guid.Parse("6B3577DF-B0F7-444A-8506-029591BC9894");
            Guid saga2Id = Guid.Parse("75E24A6E-29BF-4863-AB6A-299EB107947D");
            Guid saga1ClassId = Guid.Parse("D3F5C4A9-DC44-49F7-B629-230852265B87");
            Guid saga2ClassId = Guid.Parse("C3DDB0AD-8E4A-4CDB-9311-4AE52B167F99");

            var sagaMetadata1 = new SagaMetadataRecord(saga1Id, saga1ClassId);
            sagaMetadata1.Keys.AddRange(new[]
            {
                new SagaMetadataKey(Guid.Parse("FC9083F9-0FB1-46DE-BC50-EECD5D5B1A79"), saga1Id, "key", "value")
            });

            var sagaMetadata2 = new SagaMetadataRecord(saga2Id, saga2ClassId);
            sagaMetadata2.Keys.AddRange(new[]
            {
                new SagaMetadataKey(Guid.Parse("919274C2-E501-430D-A60D-60DFD486C0EB"), saga2Id, "key", "value")
            });

            inMemoryCrudRepository.AttachRange(sagaMetadata1.Keys.Concat(sagaMetadata2.Keys));
            inMemoryCrudRepository.AttachRange(new[]
            {
                sagaMetadata1, sagaMetadata2
            });

            SagaMatch[] result = await sut.FindSagasByKeyAsync(saga1ClassId, "key", "value");

            result.Should().BeEquivalentTo(new[]
            {
                new SagaMatch() { Id = sagaMetadata1.Id, ClassId = sagaMetadata1.ClassId }
            });
        }

        [Fact]
        public async Task FindSagasByKeyAsync_ReflectsCachedChanges()
        {
            Guid saga1Id = Guid.Parse("6B3577DF-B0F7-444A-8506-029591BC9894");
            Guid saga2Id = Guid.Parse("75E24A6E-29BF-4863-AB6A-299EB107947D");
            Guid saga3Id = Guid.Parse("03A96C89-8004-42DB-874D-D25C3D3C2476");
            Guid sagaClassId = Guid.Parse("D3F5C4A9-DC44-49F7-B629-230852265B87");

            var sagaMetadata1 = new SagaMetadataRecord(saga1Id, sagaClassId);
            sagaMetadata1.Keys.AddRange(new[]
            {
                new SagaMetadataKey(Guid.Parse("FC9083F9-0FB1-46DE-BC50-EECD5D5B1A79"), saga1Id, "key", "value")
            });

            var sagaMetadata2 = new SagaMetadataRecord(saga2Id, sagaClassId);
            sagaMetadata2.Keys.AddRange(new[]
            {
                new SagaMetadataKey(Guid.Parse("919274C2-E501-430D-A60D-60DFD486C0EB"), saga2Id, "key", "value2")
            });

            var sagaMetadata3 = new SagaMetadataRecord(saga3Id, sagaClassId);
            sagaMetadata3.Keys.AddRange(new[]
            {
                new SagaMetadataKey(Guid.Parse("919274C2-E501-430D-A60D-60DFD486C0EB"), sagaMetadata3.Id, "key", "value")
            });

            inMemoryCrudRepository.AttachRange(sagaMetadata1.Keys.Concat(sagaMetadata2.Keys).Concat(sagaMetadata3.Keys));
            inMemoryCrudRepository.AttachRange(new[]
            {
                sagaMetadata1, sagaMetadata2, sagaMetadata3
            });

            await sut.SetSagaKeysAsync(sagaMetadata2.Id, new[]
            {
                new KeyValuePair<string, string>("key", "value"),
            });

            await sut.SetSagaKeysAsync(sagaMetadata3.Id, new[]
            {
                new KeyValuePair<string, string>("key", "value3"),
            });

            SagaMatch[] result = await sut.FindSagasByKeyAsync(sagaClassId, "key", "value");

            result.Should().BeEquivalentTo(new[]
            {
                new SagaMatch() { Id = sagaMetadata1.Id, ClassId = sagaMetadata1.ClassId },
                new SagaMatch() { Id = sagaMetadata2.Id, ClassId = sagaMetadata2.ClassId }
            });
        }

        [Fact]
        public async Task GetSagaMetadataAsync_GetsFromRepo()
        {
            var sagaMetadata1 = new SagaMetadataRecord(Guid.Parse("46FA66FF-C8AC-4E24-AE74-11DAFF21D3D4"), Guid.Parse("39F01E63-CA48-463E-B9C9-F049832DE92E"));
            inMemoryCrudRepository.Attach(sagaMetadata1);

            var result = await sut.GetSagaMetadataAsync(sagaMetadata1.Id);
            result.ClassId.Should().Be(sagaMetadata1.ClassId);
        }

        [Fact]
        public async Task GetSagaMetadataAsync_GetsNewylAdded()
        {
            Guid sagaId = Guid.Parse("46FA66FF-C8AC-4E24-AE74-11DAFF21D3D4");
            Guid sagaClassId = Guid.Parse("39F01E63-CA48-463E-B9C9-F049832DE92E");

            sut.AddSaga(sagaId, sagaClassId);

            var result = await sut.GetSagaMetadataAsync(sagaId);
            result.ClassId.Should().Be(sagaClassId);
        }

        [Fact]
        public async Task SetSagaMetadataAsync_AddsNew()
        {
            var sagaMetadata1 = new SagaMetadataRecord(Guid.Parse("B3F5E8E2-2681-4F57-8F9D-5577B9198CA8"), Guid.NewGuid());
            inMemoryCrudRepository.Attach(sagaMetadata1);

            await sut.SetSagaKeysAsync(sagaMetadata1.Id, new[]
                {
                    new KeyValuePair<string, string>("key", "value")
                });

            var allMetadataKeys = inMemoryCrudRepository.FindAllWithAdded<SagaMetadataKey>().ToList();
            allMetadataKeys.Count.Should().Be(1);
            allMetadataKeys.Should().Contain(x => x.KeyName == "key" && x.KeyValue == "value" && x.SagaId == sagaMetadata1.Id);
            sagaMetadata1.Keys.Should().BeEquivalentTo(allMetadataKeys);
        }

        [Fact]
        public async Task SetSagaMetadataAsync_AddsNewMultiValue()
        {
            var sagaMetadata1 = new SagaMetadataRecord(Guid.Parse("B3F5E8E2-2681-4F57-8F9D-5577B9198CA8"), Guid.NewGuid());
            inMemoryCrudRepository.Attach(sagaMetadata1);

            await sut.SetSagaKeysAsync(sagaMetadata1.Id, new[]
            {
                new KeyValuePair<string, string>("key", "value1"),
                new KeyValuePair<string, string>("key", "value2")
            });

            var allMetadataKeys = inMemoryCrudRepository.FindAllWithAdded<SagaMetadataKey>().ToList();
            allMetadataKeys.Count.Should().Be(2);
            allMetadataKeys.Should().Contain(x => x.KeyName == "key" && x.KeyValue == "value1" && x.SagaId == sagaMetadata1.Id);
            allMetadataKeys.Should().Contain(x => x.KeyName == "key" && x.KeyValue == "value2" && x.SagaId == sagaMetadata1.Id);
            sagaMetadata1.Keys.Should().BeEquivalentTo(allMetadataKeys);
        }

        [Fact]
        public async Task SetSagaMetadataAsync_UpdatesExisting()
        {
            var sagaMetadata1 = new SagaMetadataRecord(Guid.Parse("B3F5E8E2-2681-4F57-8F9D-5577B9198CA8"), Guid.NewGuid());
            inMemoryCrudRepository.Attach(sagaMetadata1);
            sagaMetadata1.Keys.AddRange(new[]
            {
                new SagaMetadataKey(Guid.Parse("FC9083F9-0FB1-46DE-BC50-EECD5D5B1A79"), sagaMetadata1.Id, "key", "value")
            });
            inMemoryCrudRepository.AttachRange(sagaMetadata1.Keys);

            await sut.SetSagaKeysAsync(sagaMetadata1.Id, new[]
            {
                new KeyValuePair<string, string>("key", "value2")
            });
            
            await inMemoryCrudRepository.SaveChangesAsync();

            var allMetadataKeys = inMemoryCrudRepository.FindAllWithAdded<SagaMetadataKey>().ToList();
            allMetadataKeys.Count.Should().Be(1);
            allMetadataKeys.Should().Contain(x => x.KeyName == "key" && x.KeyValue == "value2" && x.SagaId == sagaMetadata1.Id);
            sagaMetadata1.Keys.Should().BeEquivalentTo(allMetadataKeys);
        }

        [Fact]
        public async Task SetSagaMetadataAsync_UpdatesSome()
        {
            var sagaMetadata1 = new SagaMetadataRecord(Guid.Parse("B3F5E8E2-2681-4F57-8F9D-5577B9198CA8"), Guid.NewGuid());
            inMemoryCrudRepository.Attach(sagaMetadata1);
            sagaMetadata1.Keys.AddRange(new[]
            {
                new SagaMetadataKey(Guid.NewGuid(), sagaMetadata1.Id, "key", "value1"),
                new SagaMetadataKey(Guid.NewGuid(), sagaMetadata1.Id, "key", "value2")
            });
            inMemoryCrudRepository.AttachRange(sagaMetadata1.Keys);
            
            await sut.SetSagaKeysAsync(sagaMetadata1.Id, new[]
            {
                new KeyValuePair<string, string>("key", "value2"),
                new KeyValuePair<string, string>("key", "value3")
            });

            await inMemoryCrudRepository.SaveChangesAsync();

            var allMetadataKeys = inMemoryCrudRepository.FindAllWithAdded<SagaMetadataKey>().ToList();
            allMetadataKeys.Count.Should().Be(2);
            allMetadataKeys.Should().Contain(x => x.KeyName == "key" && x.KeyValue == "value2" && x.SagaId == sagaMetadata1.Id);
            allMetadataKeys.Should().Contain(x => x.KeyName == "key" && x.KeyValue == "value3" && x.SagaId == sagaMetadata1.Id);
            sagaMetadata1.Keys.Should().BeEquivalentTo(allMetadataKeys);
        }

        [Fact]
        public async Task SetSagaMetadataAsync_RemovesUnmatched()
        {
            var sagaMetadata1 = new SagaMetadataRecord(Guid.Parse("B3F5E8E2-2681-4F57-8F9D-5577B9198CA8"), Guid.NewGuid());
            inMemoryCrudRepository.Attach(sagaMetadata1);
            var key = new SagaMetadataKey(Guid.Parse("FC9083F9-0FB1-46DE-BC50-EECD5D5B1A79"), sagaMetadata1.Id, "key", "value");
            sagaMetadata1.Keys.AddRange(new[]
            {
                key
            });
            inMemoryCrudRepository.AttachRange(sagaMetadata1.Keys);

            await sut.SetSagaKeysAsync(sagaMetadata1.Id, new KeyValuePair<string, string>[] {});

            inMemoryCrudRepository.GetEntityState(key).Should().Be(EntityState.Deleted);
            sagaMetadata1.Keys.Should().BeEmpty();
        }

        [Fact]
        public async Task SaveChangesAsync_SavesRepo()
        {
            var inMemoryCrudRepository = Substitute.ForPartsOf<InMemoryCrudRepository>(); // we could do this the entire fixture, but there is likely a bug in NSubstitute causing occasional failure of some tests (proxy calls base implementation CreateQueryable instead of derived)
            var sut = new SagaMetadataRepository(inMemoryCrudRepository);

            await sut.SaveChangesAsync();
            inMemoryCrudRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}
