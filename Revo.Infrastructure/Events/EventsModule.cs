using Ninject.Modules;
using Revo.Core.Core;
using Revo.Core.Lifecycle;
using Revo.Infrastructure.Events.Metadata;
using Revo.Infrastructure.Events.Upgrades;

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

            Bind<IEventStreamUpgrader>()
                .To<EventStreamUpgrader>()
                .InSingletonScope();

            Bind<IEventStreamSequenceNumbersUpgrade>()
                .To<EventStreamSequenceNumbersUpgrade>()
                .InSingletonScope();

            Bind<IApplicationConfigurer>()
                .To<EventUpgradeDiscovery>()
                .InSingletonScope();
        }
    }
}
