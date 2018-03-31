using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Revo.DataAccess.EF6.InMemory;
using Revo.DataAccess.Entities;
using Revo.DataAccess.InMemory;
using Xunit;

namespace Revo.DataAccess.EF6.Tests.InMemory
{
    public class InMemoryDbEntityEntryTests
    {
        [Fact]
        public void Entity_GetsEntityInstance()
        {
            var entityEntry = new InMemoryCrudRepository.EntityEntry(new Entity1(), EntityState.Unchanged);
            InMemoryDbEntityEntry sut = new InMemoryDbEntityEntry(entityEntry);

            sut.Entity.Should().Be(entityEntry.Instance);
        }

        [Theory]
        [InlineData(EntityState.Modified, System.Data.Entity.EntityState.Modified)]
        [InlineData(EntityState.Added, System.Data.Entity.EntityState.Added)]
        [InlineData(EntityState.Deleted, System.Data.Entity.EntityState.Deleted)]
        [InlineData(EntityState.Detached, System.Data.Entity.EntityState.Detached)]
        [InlineData(EntityState.Unchanged, System.Data.Entity.EntityState.Unchanged)]
        public void State_GetsMapped(EntityState revoState, System.Data.Entity.EntityState efState)
        {
            var entityEntry = new InMemoryCrudRepository.EntityEntry(new Entity1(), revoState);
            InMemoryDbEntityEntry sut = new InMemoryDbEntityEntry(entityEntry);
            sut.State.Should().Be(efState);
        }

        [Theory]
        [InlineData(EntityState.Modified, System.Data.Entity.EntityState.Modified)]
        [InlineData(EntityState.Added, System.Data.Entity.EntityState.Added)]
        [InlineData(EntityState.Deleted, System.Data.Entity.EntityState.Deleted)]
        [InlineData(EntityState.Detached, System.Data.Entity.EntityState.Detached)]
        [InlineData(EntityState.Unchanged, System.Data.Entity.EntityState.Unchanged)]
        public void State_SetsMapped(EntityState revoState, System.Data.Entity.EntityState efState)
        {
            var entityEntry = new InMemoryCrudRepository.EntityEntry(new Entity1(), EntityState.Unchanged);
            InMemoryDbEntityEntry sut = new InMemoryDbEntityEntry(entityEntry);
            sut.State = efState;
            entityEntry.State.Should().Be(revoState);
        }

        [Fact]
        public void Collection_CurrentValueGetsCollection()
        {
            var entity = new Entity1();
            var entityEntry = new InMemoryCrudRepository.EntityEntry(entity, EntityState.Unchanged);
            InMemoryDbEntityEntry<Entity1> sut = new InMemoryDbEntityEntry<Entity1>(entityEntry);

            sut.Collection(x => x.Children).CurrentValue.Should().BeSameAs(entity.Children);
        }

        [Fact]
        public void Reference_CurrentValueGetsProperty()
        {
            var entity = new Entity1();
            var entityEntry = new InMemoryCrudRepository.EntityEntry(entity, EntityState.Unchanged);
            InMemoryDbEntityEntry<Entity1> sut = new InMemoryDbEntityEntry<Entity1>(entityEntry);

            sut.Reference(x => x.Other).CurrentValue.Should().BeSameAs(entity.Other);
        }

        public class Entity1
        {
            public int Foo { get; set; }
            public Guid Bar { get; set; }
            public Entity1 Other { get; set; }
            public List<Entity2> Children { get; set; }
        }

        public class Entity2
        {
            public decimal Boo { get; set; }
        }
    }
}
