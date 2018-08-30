using Ninject.Modules;
using Revo.Core.Core;
using Revo.EF6.DataAccess.Entities;
using Revo.Infrastructure.Sagas;

namespace Revo.EF6.Sagas
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
