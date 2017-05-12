using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace GTRevo.DataAccess.EF6
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
