using Ninject.Modules;
using Revo.Core.Core;
using Revo.DataAccess.EF6.Entities;
using Revo.Infrastructure.Sagas;

namespace Revo.Infrastructure.EF6.Sagas
{
    [AutoLoadModule(false)]
    public class EF6SagasModule : NinjectModule
    {
        public override void Load()
        {
            Bind<ISagaMetadataRepository>()
                .To<EF6SagaMetadataRepository>()
                .InRequestOrJobScope();

            Bind<IEF6CrudRepository>()
                .To<EF6CrudRepository>()
                .WhenInjectedInto<EF6SagaMetadataRepository>()
                .InTransientScope();
        }
    }
}
