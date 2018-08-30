using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace Revo.EF6.DataAccess.Entities
{
    public class EntityContext : DbContext
    {
        public EntityContext(string nameOrConnectionString,
            DbCompiledModel compiledModel)
            : base(nameOrConnectionString, compiledModel)
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
