using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using Ninject.Modules;
using Revo.Core.Core;
using Revo.Core.Lifecycle;
using Revo.DataAccess.Entities;
using Revo.EF6.DataAccess.Entities;
using Revo.EF6.DataAccess.Model;

namespace Revo.EF6.DataAccess
{
    [AutoLoadModule(false)]
    public class EF6DataAccessModule : NinjectModule
    {
        private readonly EF6DataAccessConfigurationSection configurationSection;

        public EF6DataAccessModule(EF6DataAccessConfigurationSection configurationSection)
        {
            this.configurationSection = configurationSection;
        }

        public override void Load()
        {
            Bind<EF6ConnectionConfiguration>()
                .ToConstant(configurationSection.Connection);
            
            List<Type> repositoryTypes = new List<Type>()
            {
                typeof(IEF6CrudRepository),
                typeof(IEF6ReadRepository)
            };

            if (configurationSection.UseAsPrimaryRepository)
            {
                repositoryTypes.AddRange(new[]
                {
                    typeof(ICrudRepository), typeof(IReadRepository)
                });
            }

            Bind(repositoryTypes.ToArray())
                .To<EF6CrudRepository>()
                .InTaskScope();

            Bind(repositoryTypes.Select(x => typeof(ICrudRepositoryFactory<>).MakeGenericType(x)).ToArray())
                .To<EF6CrudRepositoryFactory>()
                .InTaskScope();

            Bind<IEF6DatabaseAccess>().To<EF6DatabaseAccess>()
                .InTaskScope();

            Bind<IRequestDbContextCache>()
                .To<RequestDbContextCache>()
                .InRequestScope();

            Bind<IModelMetadataExplorer, IApplicationStartedListener>()
                .To<ModelMetadataExplorer>()
                .InSingletonScope();

            Bind<ModelDefinitionDiscovery>()
                .ToSelf()
                .InSingletonScope();

            Bind<EntityTypeDiscovery>()
                .ToSelf()
                .InSingletonScope();

            Bind<IDbContextFactory, IApplicationStartedListener>()
                .To<DbContextFactory>()
                .InSingletonScope();

            if (configurationSection.ConventionTypes != null)
            {
                foreach (var conventionType in configurationSection.ConventionTypes)
                {
                    Bind<IConvention>().To(conventionType);
                }
            }
        }
    }
}
