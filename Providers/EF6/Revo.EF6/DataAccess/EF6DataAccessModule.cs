using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Ninject.Extensions.ContextPreservation;
using Ninject.Modules;
using Revo.Core.Core;
using Revo.Core.Lifecycle;
using Revo.DataAccess.Entities;
using Revo.EF6.DataAccess.Entities;
using Revo.Infrastructure.EF6.DataAcccess.Model;

namespace Revo.EF6.DataAccess
{
    [AutoLoadModule(false)]
    public class EF6DataAccessModule : NinjectModule
    {
        private readonly EF6ConnectionConfiguration connectionConfiguration;
        private readonly bool useAsPrimaryRepository;

        public EF6DataAccessModule(EF6ConnectionConfiguration connectionConfiguration, bool useAsPrimaryRepository)
        {
            this.connectionConfiguration = connectionConfiguration;
            this.useAsPrimaryRepository = useAsPrimaryRepository;
        }

        public override void Load()
        {
            Bind<EF6ConnectionConfiguration>()
                .ToConstant(connectionConfiguration);
            
            List<Type> repositoryTypes = new List<Type>()
            {
                typeof(IEF6CrudRepository),
                typeof(IEF6ReadRepository)
            };

            if (useAsPrimaryRepository)
            {
                repositoryTypes.AddRange(new[]
                {
                    typeof(ICrudRepository), typeof(IReadRepository)
                });

                Bind<IDatabaseAccess>().To<DatabaseAccess>()
                    .InRequestOrJobScope();
            }

            Bind(repositoryTypes.ToArray())
                .To<EF6CrudRepository>()
                .InRequestOrJobScope();

            Bind(repositoryTypes.Select(x => typeof(ICrudRepositoryFactory<>).MakeGenericType(x)).ToArray())
                .To<EF6CrudRepositoryFactory>()
                .InRequestOrJobScope();

            Bind<IModelMetadataExplorer, IApplicationStartListener>()
                .To<ModelMetadataExplorer>()
                .InSingletonScope();

            Bind<ModelDefinitionDiscovery>()
                .ToSelf()
                .InSingletonScope();

            Bind<EntityTypeDiscovery>()
                .ToSelf()
                .InSingletonScope();

            Bind<IDbContextFactory, IApplicationStartListener>()
                .To<DbContextFactory>()
                .InSingletonScope();
        }
    }
}
