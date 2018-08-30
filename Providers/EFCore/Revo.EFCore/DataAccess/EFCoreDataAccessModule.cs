using System;
using System.Collections.Generic;
using System.Linq;
using Ninject.Modules;
using Revo.Core.Core;
using Revo.Core.Lifecycle;
using Revo.DataAccess.Entities;
using Revo.EFCore.DataAccess.Entities;
using Revo.EFCore.DataAccess.Model;

namespace Revo.EFCore.DataAccess
{
    public class EFCoreDataAccessModule : NinjectModule
    {
        private readonly bool useAsPrimaryRepository;

        public EFCoreDataAccessModule(bool useAsPrimaryRepository)
        {
            this.useAsPrimaryRepository = useAsPrimaryRepository;
        }

        public override void Load()
        {
            List<Type> repositoryTypes = new List<Type>()
            {
                typeof(IEFCoreCrudRepository),
                typeof(IEFCoreReadRepository)
            };

            if (useAsPrimaryRepository)
            {
                repositoryTypes.AddRange(new[]
                {
                    typeof(ICrudRepository), typeof(IReadRepository)
                });
            }

            Bind(repositoryTypes.ToArray())
                .To<EFCoreCrudRepository>()
                .InRequestOrJobScope();

            Bind(repositoryTypes.Select(x => typeof(ICrudRepositoryFactory<>).MakeGenericType(x)).ToArray())
                .To<EFCoreCrudRepositoryFactory>()
                .InRequestOrJobScope();

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
