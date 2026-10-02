using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using NSubstitute;
using Revo.DataAccess.Entities;
using Revo.EF6.DataAccess.Entities;
using Xunit;
using EntityState = System.Data.Entity.EntityState;

namespace Revo.EF6.Tests.DataAccess.Entities
{
    public class EF6CrudRepositoryTests
    {
        [Theory]
        [InlineData(EntityState.Added)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Deleted)]
        public void SaveChanges_AppliesWriteFiltersBeforeSaving(EntityState state)
        {
            var filter = new WriteFilter();
            using var context = new TestContext(filter);
            var entity = new TestEntity { Id = 1 };
            context.Entry(entity).State = state;
            var databaseAccess = Substitute.For<IEF6DatabaseAccess>();
            databaseAccess.DbContexts.Returns(new Dictionary<string, DbContext> { ["test"] = context });
            var repository = new EF6CrudRepository(databaseAccess, new IRepositoryFilter[] { filter });

            repository.SaveChanges();

            Assert.Equal(state, filter.FilteredState);
            Assert.Same(entity, filter.FilteredEntity);
            Assert.True(context.SavedAfterFiltering);
        }

        private sealed class TestContext : DbContext
        {
            private readonly WriteFilter filter;

            static TestContext()
            {
                Database.SetInitializer<TestContext>(null);
            }

            public TestContext(WriteFilter filter)
                : base(new SqlConnection("Data Source=unused;Initial Catalog=unused;Integrated Security=True"), CreateModel(), true)
            {
                this.filter = filter;
            }

            public bool SavedAfterFiltering { get; private set; }

            public override int SaveChanges()
            {
                SavedAfterFiltering = filter.FilteredEntity != null;
                return 0;
            }

            private static DbCompiledModel CreateModel()
            {
                var builder = new DbModelBuilder();
                builder.Entity<TestEntity>().HasKey(entity => entity.Id);
                return builder.Build(new DbProviderInfo("System.Data.SqlClient", "2012")).Compile();
            }
        }

        private sealed class TestEntity
        {
            public int Id { get; set; }
        }

        private sealed class WriteFilter : IRepositoryFilter
        {
            public EntityState? FilteredState { get; private set; }
            public object FilteredEntity { get; private set; }
            public IQueryable<T> FilterResults<T>(IQueryable<T> results) where T : class => results;
            public T FilterResult<T>(T result) where T : class => result;
            public void FilterAdded<T>(T entity) where T : class => Record(entity, EntityState.Added);
            public void FilterModified<T>(T entity) where T : class => Record(entity, EntityState.Modified);
            public void FilterDeleted<T>(T entity) where T : class => Record(entity, EntityState.Deleted);

            private void Record(object entity, EntityState state)
            {
                FilteredEntity = entity;
                FilteredState = state;
            }
        }
    }
}
