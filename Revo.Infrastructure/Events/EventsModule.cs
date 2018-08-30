using Ninject.Modules;
using Revo.Core.Core;
using Revo.Infrastructure.Events.Metadata;

namespace Revo.Infrastructure.Events
{
    public class EventsModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IEventMessageFactory>()
                .To<EventMessageFactory>()
                .InRequestOrJobScope();

            Bind<IEventMetadataProvider>()
                .To<ActorNameEventMetadataProvider>()
                .InRequestOrJobScope();

            Bind<IEventMetadataProvider>()
                .To<MachineNameEventMetadataProvider>()
                .InSingletonScope();
            
            Bind<IEventMetadataProvider>()
                .To<UserIdEventMetadataProvider>()
                .InRequestOrJobScope();
            
            Bind<IEventMetadataProvider>()
                .To<CommandContextEventMetadataProvider>()
                .InRequestOrJobScope();
        }
    }
}
