using Ninject.Modules;
using Revo.Core.Core;
using Revo.EFCore.DataAccess.Entities;
using Revo.Infrastructure.Sagas;
using Revo.Infrastructure.Sagas.Generic;

namespace Revo.EFCore.Sagas
{
    [AutoLoadModule(false)]
    public class EFCoreSagasModule : NinjectModule
    {
        public override void Load()
        {
            Bind<ISagaMetadataRepository>()
                .To<SagaMetadataRepository>()
                .InRequestOrJobScope();

            Bind<IEFCoreCrudRepository>()
                .To<EFCoreCrudRepository>()
                .WhenInjectedInto<SagaMetadataRepository>()
                .InTransientScope();
        }
    }
}
