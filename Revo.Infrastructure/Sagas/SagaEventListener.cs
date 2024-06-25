using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Revo.Core.Commands;
using Revo.Core.Events;
using Revo.Core.Transactions;
using Revo.Domain.Events;
using Revo.Infrastructure.Events.Async;

namespace Revo.Infrastructure.Sagas
{
    public class SagaEventListener(SagaEventSequencer sagaEventSequencer,
            Func<ISagaEventDispatcher> sagaEventDispatcherFunc, CommandContextStack commandContextStack,
            IUnitOfWorkFactory unitOfWorkFactory) :
        IAsyncEventListener<DomainEvent>
    {
        private readonly List<IEventMessage<DomainEvent>> bufferedEvents = new List<IEventMessage<DomainEvent>>();

        public Task HandleAsync(IEventMessage<DomainEvent> message, string sequenceName)
        {
            bufferedEvents.Add(message);
            return Task.FromResult(0);
        }

        public IAsyncEventSequencer EventSequencer { get; }

        public Task OnFinishedEventQueueAsync(string sequenceName)
        {
            return DispatchSagaEvents();
        }

        private async Task DispatchSagaEvents()
        {
            using (IUnitOfWork uow = unitOfWorkFactory.CreateUnitOfWork())
            {
                commandContextStack.Push(new CommandContext(null, uow));

                try
                {
                    uow.Begin();

                    var sagaEventDispatcher = sagaEventDispatcherFunc();
                    await sagaEventDispatcher.DispatchEventsToSagas(bufferedEvents);

                    await uow.CommitAsync();
                    bufferedEvents.Clear();
                }
                finally
                {
                    commandContextStack.Pop();
                }
            }
        }
    }
}
