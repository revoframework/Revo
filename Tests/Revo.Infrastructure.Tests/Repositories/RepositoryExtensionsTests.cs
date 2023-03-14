using System;
using System.Linq;
using System.Threading.Tasks;
using Revo.Domain.Entities.Basic;
using Revo.Infrastructure.Repositories;
using Revo.Testing.Infrastructure.Repositories;
using Xunit;

namespace Revo.Infrastructure.Tests.Repositories
{
    public class RepositoryExtensionsTests
    {
        private readonly FakeRepository repository;

        public RepositoryExtensionsTests()
        {
            repository = new FakeRepository();
        }

        [Fact]
        public async Task AddIfNew_AddsSingleById()
        {
            TestEntity entity = new TestEntity(Guid.NewGuid());
            TestEntity result = await RepositoryExtensions.AddIfNewAsync(repository, entity);
            repository.SaveChanges();

            Assert.Equal(entity, result);
            Assert.Equal(1, repository.FindAll<TestEntity>().Count());
            Assert.Contains(repository.FindAll<TestEntity>(),
                x => x == entity);
        }

        [Fact]
        public async Task AddIfNew_DoesntAddSingleDuplicateById()
        {
            TestEntity entity1 = new TestEntity(Guid.NewGuid());
            repository.Add(entity1);
            repository.SaveChanges();

            TestEntity entity2 = new TestEntity(entity1.Id);
            TestEntity result = await RepositoryExtensions.AddIfNewAsync(repository, entity2);
            repository.SaveChanges();

            Assert.Equal(entity1, result);
            Assert.Equal(1, repository.FindAll<TestEntity>().Count());
            Assert.Contains(repository.FindAll<TestEntity>(),
                x => x == entity1);
        }
        
        [Fact]
        public async Task AddIfNew_DoesntAddSingleDuplicateByProperty()
        {
            TestEntity entity1 = new TestEntity(Guid.NewGuid()) { Value = "Foo" };
            repository.Add(entity1);
            repository.SaveChanges();

            TestEntity entity2 = new TestEntity(Guid.NewGuid()) { Value = "Foo" };
            TestEntity result = await RepositoryExtensions.AddIfNewAsync(repository, x => x.Value, entity2);
            repository.SaveChanges();

            Assert.Equal(entity1, result);
            Assert.Equal(1, repository.FindAll<TestEntity>().Count());
            Assert.Contains(repository.FindAll<TestEntity>(),
                x => x == entity1);
        }

        [Fact]
        public async Task AddIfNew_AddsSecondByProperty()
        {
            TestEntity entity1 = new TestEntity(Guid.NewGuid()) { Value = "Foo" };
            repository.Add(entity1);
            repository.SaveChanges();

            TestEntity entity2 = new TestEntity(Guid.NewGuid()) { Value = "Bar" };
            TestEntity result = await RepositoryExtensions.AddIfNewAsync(repository, x => x.Value, entity2);
            repository.SaveChanges();

            Assert.Equal(entity2, result);
            Assert.Equal(2, repository.FindAll<TestEntity>().Count());
            Assert.Contains(repository.FindAll<TestEntity>(),
                x => x == entity1);
            Assert.Contains(repository.FindAll<TestEntity>(),
                x => x == entity2);
        }

        public class TestEntity : BasicAggregateRoot
        {
            public TestEntity(Guid id) : base(id)
            {
            }

            protected TestEntity()
            {
            }

            public string Value { get; set; }
        }
    }
}
