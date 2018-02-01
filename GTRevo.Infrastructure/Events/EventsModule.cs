using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Core.Core;
using GTRevo.Infrastructure.Events.Metadata;
using Ninject.Modules;

namespace GTRevo.Infrastructure.Events
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
