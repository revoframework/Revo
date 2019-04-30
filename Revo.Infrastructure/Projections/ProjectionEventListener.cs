using System.Collections.Generic;
using System.Threading.Tasks;
using Revo.Core.Commands;
using Revo.Core.Events;
using Revo.Core.Transactions;
using Revo.Domain.Events;
using Revo.Infrastructure.Events.Async;

namespace Revo.Infrastructure.Projections
{
    public abstract class ProjectionEventListener :
        IAsyncEventListener<DomainAggregateEvent>
    {
        private readonly IProjectionSubSystem projectionSubSystem;
        private readonly IUnitOfWorkFactory unitOfWorkFactory;
        private readonly CommandContextStack commandContextStack;
        private readonly List<IEventMessage<DomainAggregateEvent>> events = new List<IEventMessage<DomainAggregateEvent>>();

        public ProjectionEventListener(IProjectionSubSystem projectionSubSystem,
            IUnitOfWorkFactory unitOfWorkFactory, CommandContextStack commandContextStack)
        {
            this.projectionSubSystem = projectionSubSystem;
            this.unitOfWorkFactory = unitOfWorkFactory;
            this.commandContextStack = commandContextStack;
        }

        public abstract IAsyncEventSequencer EventSequencer { get; }

        public Task HandleAsync(IEventMessage<DomainAggregateEvent> message, string sequenceName)
        {
            events.Add(message);
            return Task.FromResult(0);
        }

        public Task OnFinishedEventQueueAsync(string sequenceName)
        {
            return ExecuteProjectionsAsync();
        }

        public async Task ExecuteProjectionsAsync()
        {
            using (IUnitOfWork uow = unitOfWorkFactory.CreateUnitOfWork())
            {
                commandContextStack.Push(new CommandContext(null, uow));

                try
                {
                    uow.Begin();

                    await projectionSubSystem.ExecuteProjectionsAsync(events, uow, GetEventProjectionOptions());
                    events.Clear();

                    await uow.CommitAsync();
                }
                finally
                {
                    commandContextStack.Pop();
                }
            }
        }

        protected virtual EventProjectionOptions GetEventProjectionOptions()
        {
            return new EventProjectionOptions();
        }
    }
}
