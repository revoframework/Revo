using Ninject.Modules;
using Revo.Core.Core;
using Revo.Infrastructure.Events;
using Revo.Infrastructure.Events.Async;

namespace Revo.EFCore.Events
{
    [AutoLoadModule(false)]
    public class EFCoreAsyncEventsModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IAsyncEventQueueManager>()
                .To<EFCoreAsyncEventQueueManager>()
                .InTaskScope();
            
            Bind<IEventSerializer>()
                .To<EventSerializer>()
                .InSingletonScope();
        }
    }
}
