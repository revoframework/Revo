using FluentAssertions;
using Revo.DataAccess.InMemory;
using Revo.Domain.ReadModel;
using System;
using Xunit;

namespace Revo.DataAccess.Tests.InMemory
{
    public class InMemoryCrudRepositoryTests
    {
        private InMemoryCrudRepository sut = new InMemoryCrudRepository();
        
        [Fact]
        public void FindAll_ReturnsAfterSaving()
        {
            var entity1 = new Entity1() { Id = Guid.NewGuid() };
            sut.Add(entity1);
            sut.SaveChanges();
            sut.FindAll<Entity1>().Should().HaveCount(1).And.Contain(entity1);
        }

        [Fact]
        public void FindAll_ReturnsOnlySaved()
        {
            var entity1 = new Entity1() { Id = Guid.NewGuid() };
            sut.Add(entity1);
            sut.FindAll<Entity1>().Should().BeEmpty();
        }
        
        [Fact]
        public void FirstOrDefault_ReturnsMatching()
        {
            var entity1 = new Entity1() { Id = Guid.NewGuid() };
            var entity2 = new Entity1() { Id = Guid.NewGuid() };
            sut.AddRange(new[] { entity1, entity2 });
            sut.SaveChanges();
            sut.FirstOrDefault<Entity1>(x => x.Id == entity1.Id).Should().Be(entity1);
        }

        [Fact]
        public void FirstOrDefault_NullIfNotFound()
        {
            sut.FirstOrDefault<Entity1>(x => x.Id == Guid.NewGuid()).Should().BeNull();
        }

        [Fact]
        public void FirstOrDefault_ReturnsOnlySaved()
        {
            var entity1 = new Entity1() { Id = Guid.NewGuid() };
            sut.Add(entity1);
            sut.FirstOrDefault<Entity1>(x => x.Id == entity1.Id).Should().BeNull();
        }

        class Entity1 : EntityReadModel {}
    }
}
