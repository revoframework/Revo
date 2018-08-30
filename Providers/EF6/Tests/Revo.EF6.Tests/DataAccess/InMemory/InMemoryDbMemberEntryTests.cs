using System;
using FluentAssertions;
using Revo.DataAccess.Entities;
using Revo.DataAccess.InMemory;
using Revo.EF6.DataAccess.InMemory;
using Xunit;

namespace Revo.EF6.Tests.DataAccess.InMemory
{
    public class InMemoryDbMemberEntryTests
    {
        [Fact]
        public void Name_ReturnsPropertyName()
        {
            var entityEntry = new InMemoryCrudRepository.EntityEntry(new Entity1() { Foo = 5 }, EntityState.Unchanged);
            InMemoryDbMemberEntry<Entity1, int> sut = new InMemoryDbMemberEntry<Entity1, int>(x => x.Foo, entityEntry);

            sut.Name.Should().Be("Foo");
        }

        [Fact]
        public void CurrentValue_GetsMemberValue()
        {
            var entityEntry = new InMemoryCrudRepository.EntityEntry(new Entity1() { Foo = 5 }, EntityState.Unchanged);
            InMemoryDbMemberEntry<Entity1, int> sut = new InMemoryDbMemberEntry<Entity1, int>(x => x.Foo, entityEntry);

            sut.CurrentValue.Should().Be(5);
        }

        [Fact]
        public void CurrentValue_SetsMemberValue()
        {
            var entity = new Entity1() {Foo = 5};
            var entityEntry = new InMemoryCrudRepository.EntityEntry(entity, EntityState.Unchanged);
            InMemoryDbMemberEntry<Entity1, int> sut = new InMemoryDbMemberEntry<Entity1, int>(x => x.Foo, entityEntry);
            sut.CurrentValue = 10;

            entity.Foo.Should().Be(10);
        }

        public class Entity1
        {
            public int Foo { get; set; }
            public Guid Bar { get; set; }
        }
    }
}
