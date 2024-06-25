using System;
using System.Collections.Generic;
using System.Linq;
using Ninject.Modules;
using Revo.Core.Core;
using Revo.DataAccess.Entities;
using Revo.EFCore.DataAccess.Configuration;
using Revo.EFCore.DataAccess.Conventions;
using Revo.EFCore.DataAccess.Entities;
using Revo.EFCore.DataAccess.Model;
using Revo.EFCore.DataAccess.Query;

namespace Revo.EFCore.DataAccess
{
    public class EFCoreDataAccessModule : NinjectModule
    {
        private readonly EFCoreDataAccessConfigurationSection configurationSection;

        public EFCoreDataAccessModule(EFCoreDataAccessConfigurationSection configurationSection)
        {
            this.configurationSection = configurationSection;
        }
        
        public override void Load()
        {
            List<Type> repositoryTypes = new List<Type>()
            {
                typeof(IEFCoreCrudRepository),
                typeof(IEFCoreReadRepository)
            };

            if (configurationSection.UseAsPrimaryRepository)
            {
                repositoryTypes.AddRange(new[]
                {
                    typeof(ICrudRepository), typeof(IReadRepository)
                });
            }

            Bind(repositoryTypes.ToArray())
                .To<EFCoreCrudRepository>()
                .InTaskScope();

            Bind<IRequestDbContextCache>()
                .To<RequestDbContextCache>()
                .InRequestScope();

            Bind(repositoryTypes.Select(x => typeof(ICrudRepositoryFactory<>).MakeGenericType(x)).ToArray())
                .To<EFCoreCrudRepositoryFactory>()
                .InTaskScope();

            Bind<IEFCoreDatabaseAccess>()
                .To<EFCoreDatabaseAccess>()
                .InTaskScope();

            Bind<ModelDefinitionDiscovery>()
                .ToSelf()
                .InSingletonScope();

            Bind<EntityTypeDiscovery>()
                .ToSelf()
                .InSingletonScope();

            Bind<IDbContextFactory>()
                .To<DbContextFactory>()
                .InSingletonScope();
            
            Bind<EFCoreDataAccessConfigurationSection>().ToConstant(configurationSection);
            
            if (configurationSection.EnableCustomQueryProvider)
            {
                Bind<IEFCoreConfigurer>().To<CustomQueryProviderConfigurer>();
            }

            if (configurationSection.EnableCustomQueryTranslationPreprocessor)
            {
                Bind<IEFCoreConfigurer>().To<CustomQueryTranslationConfigurer>();
            }

            foreach (var conventionFunc in configurationSection.Conventions)
            {
                Bind<IEFCoreConvention>().ToMethod(conventionFunc);
            }

            if (configurationSection.Configurer != null)
            {
                Bind<IEFCoreConfigurer>().ToConstant(new ActionConfigurer(configurationSection.Configurer));
            }
        }
    }
}
