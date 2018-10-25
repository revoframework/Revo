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
                .InTaskScope();

            Bind<IEventMetadataProvider>()
                .To<ActorNameEventMetadataProvider>()
                .InTaskScope();

            Bind<IEventMetadataProvider>()
                .To<MachineNameEventMetadataProvider>()
                .InSingletonScope();
            
            Bind<IEventMetadataProvider>()
                .To<UserIdEventMetadataProvider>()
                .InTaskScope();
            
            Bind<IEventMetadataProvider>()
                .To<CommandContextEventMetadataProvider>()
                .InTaskScope();
        }
    }
}
