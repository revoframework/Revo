using GTRevo.Platform.Events;
using MediatR;

namespace GTRevo.Platform.Commands
{
    public class CommandBus : Mediator, ICommandBus, IEventBus
    {
        public CommandBus(SingleInstanceFactory singleInstanceFactory,
            MultiInstanceFactory multiInstanceFactory)
            : base(singleInstanceFactory, multiInstanceFactory)
        {
        }
    }
}
