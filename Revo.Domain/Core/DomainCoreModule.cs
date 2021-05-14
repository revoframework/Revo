using System.Linq;
using Ninject.Modules;
using Revo.Core.Core;
using Revo.Core.Lifecycle;
using Revo.Domain.Entities;
using Revo.Domain.Events;
using Revo.Domain.Sagas;

namespace Revo.Domain.Core
{
    public class DomainCoreModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IApplicationStartedListener>()
                .To<ConventionEventApplyRegistratorCache>()
                .InSingletonScope();

            Bind<ISagaConventionConfigurationCache, IApplicationStartedListener>()
                .To<SagaConventionConfigurationCache>()
                .InSingletonScope();

            if (!Kernel.GetBindings(typeof(IEntityTypeManager)).Any())
            {
                Bind<IEntityTypeManager, IApplicationStartedListener>()
                    .To<EntityTypeManager>()
                    .InTaskScope();
            }
        }
    }
}
