using System;
using Microsoft.EntityFrameworkCore;
using Revo.DataAccess.Entities;
using Revo.EFCore.DataAccess.Entities;
using Xunit;

namespace Revo.EFCore.Tests.DataAccess.Entities
{
    public class EFCoreCrudRepositoryTests
    {
        [Fact]
        public void SaveChanges_AppliesWriteFilters()
        {
            var context = new TestContext();
            var databaseAccess = new EFCoreDatabaseAccess(new TestDbContextFactory(context), new RequestDbContextCache());
            var filter = new WriteFilter();
            var repository = new EFCoreCrudRepository(new IRepositoryFilter[] { filter }, databaseAccess);

            repository.Add(new TestEntity { Id = 1 });
            repository.SaveChanges();

            Assert.True(filter.Added);
        }

        private sealed class TestContext : DbContext
        {
            public TestContext()
                : base(new DbContextOptionsBuilder<TestContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options)
            {
            }

            public DbSet<TestEntity> Entities { get; set; }
        }

        private sealed class TestEntity
        {
            public int Id { get; set; }
        }

        private sealed class TestDbContextFactory : IDbContextFactory
        {
            private readonly DbContext context;

            public TestDbContextFactory(DbContext context)
            {
                this.context = context;
            }

            public DbContext CreateContext(string schemaSpace) => context;
        }

        private sealed class WriteFilter : IRepositoryFilter
        {
            public bool Added { get; private set; }

            public System.Linq.IQueryable<T> FilterResults<T>(System.Linq.IQueryable<T> results) where T : class => results;
            public T FilterResult<T>(T result) where T : class => result;
            public void FilterAdded<T>(T added) where T : class => Added = true;
            public void FilterDeleted<T>(T deleted) where T : class { }
            public void FilterModified<T>(T modified) where T : class { }
        }
    }
}
