using Ninject.Modules;
using Revo.Core.Core;
using Revo.Infrastructure.Sagas;

namespace Revo.Infrastructure.EF6.Sagas
{
    public class EF6SagasModule : NinjectModule
    {
        public override void Load()
        {
            Bind<ISagaMetadataRepository>()
                .To<EF6SagaMetadataRepository>()
                .InRequestOrJobScope();
        }
    }
}
