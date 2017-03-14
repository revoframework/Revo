using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace GTRevo.DataAccess.EF6
{
    public class EntityContext : DbContext
    {
        private readonly ModelDefinitionDiscovery modelDefinitionDiscovery;

        public EntityContext(/*string nameOrConnectionString,*/
            ModelDefinitionDiscovery modelDefinitionDiscovery) : base("EFConnectionString" /*nameOrConnectionString*/)
        {
            this.modelDefinitionDiscovery = modelDefinitionDiscovery;
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            modelBuilder.Conventions.Add(new CustomStoreConvention());

            IEnumerable<IModelDefinition> modelDefinitions = modelDefinitionDiscovery.DiscoverModelDefinitions();

            foreach (IModelDefinition modelDefinition in modelDefinitions)
            {
                modelDefinition.OnModelCreating(modelBuilder);
            }
        }
    }
}
