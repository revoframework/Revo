using GTRevo.Core.Events;
using MediatR;

namespace GTRevo.Core.Commands
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
