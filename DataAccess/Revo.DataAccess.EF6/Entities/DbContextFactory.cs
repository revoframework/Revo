using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using Revo.Core.Lifecycle;
using Revo.DataAccess.EF6.Model;

namespace Revo.DataAccess.EF6.Entities
{
    public class DbContextFactory : IDbContextFactory, IApplicationStartListener
    {
        private readonly ModelDefinitionDiscovery modelDefinitionDiscovery;
        private readonly EntityTypeDiscovery entityTypeDiscovery;
        private readonly IConvention[] conventions;
        private Dictionary<string, DbCompiledModel> dbModels;
        private MultiValueDictionary<string, Type> schemaSpacesToEntities;

        public DbContextFactory(ModelDefinitionDiscovery modelDefinitionDiscovery,
            IConvention[] conventions, EntityTypeDiscovery entityTypeDiscovery)
        {
            this.modelDefinitionDiscovery = modelDefinitionDiscovery;
            this.conventions = conventions;
            this.entityTypeDiscovery = entityTypeDiscovery;
        }

        public DbContext CreateContext(string schemaSpace)
        {
            EnsureLoaded();

            DbCompiledModel compiledModel;
            if (dbModels.TryGetValue(schemaSpace, out compiledModel))
            {
                return new EntityContext(compiledModel);
            }

            throw new ArgumentException("Unknown entity schema space: " + schemaSpace);
        }

        public void OnApplicationStarted()
        {
            EnsureLoaded();
        }

        private void BuildDbModels()
        {
            dbModels = new Dictionary<string, DbCompiledModel>();
            schemaSpacesToEntities = entityTypeDiscovery.DiscoverEntities();
            foreach (string schemaSpace in schemaSpacesToEntities.Keys)
            {
                DbModel model = BuildDbModel(schemaSpace);
                if (model != null)
                {
                    dbModels.Add(schemaSpace, model.Compile());
                }
            }
        }

        private DbModel BuildDbModel(string schemaSpace)
        {
            DbModelBuilder modelBuilder = new DbModelBuilder();

            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            modelBuilder.Conventions.Add(conventions);

            foreach (CustomStoreConvention convention in conventions.OfType<CustomStoreConvention>())
            {
                convention.CurrentSchemaSpace = schemaSpace;
                convention.Reset();
            }
            
            IReadOnlyCollection<Type> entityTypes = schemaSpacesToEntities[schemaSpace];
            foreach (Type entityType in entityTypes)
            {
                modelBuilder.RegisterEntityType(entityType);
            }

            new PrivateCollectionMapping().MapPrivateCollections(modelBuilder); // TODO convert this to an EF convention

            IEnumerable<IModelDefinition> modelDefinitions = modelDefinitionDiscovery.DiscoverModelDefinitions();
            foreach (IModelDefinition modelDefinition in modelDefinitions)
            {
                modelDefinition.OnModelCreating(modelBuilder);
            }

            var connectionString = ConfigurationManager.ConnectionStrings["EFConnectionString"]?.ConnectionString;
            if (connectionString?.Length > 0)
            {
                DbConnection dbConnection = Database.DefaultConnectionFactory.CreateConnection(connectionString);
                return modelBuilder.Build(dbConnection);
            }

            return null;
        }

        private void EnsureLoaded()
        {
            if (dbModels == null)
            {
                BuildDbModels();
            }
        }
    }
}
