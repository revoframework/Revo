using GTRevo.Events;
using MediatR;

namespace GTRevo.Commands
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
