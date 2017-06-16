using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace GTRevo.DataAccess.EF6.Entities
{
    public class EntityContext : DbContext
    {
        public EntityContext(/*string nameOrConnectionString,*/
            DbCompiledModel compiledModel)
            : base("EFConnectionString", compiledModel /*nameOrConnectionString*/)
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
