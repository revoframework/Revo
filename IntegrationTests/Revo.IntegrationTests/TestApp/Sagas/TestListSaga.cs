using System;
using Revo.Core.Events;
using Revo.Domain.Entities.Attributes;
using Revo.Domain.Sagas;
using Revo.Domain.Sagas.Attributes;
using Revo.IntegrationTests.TestApp.Domain.Events;
using Revo.IntegrationTests.TestApp.Messages.Commands;

namespace Revo.IntegrationTests.TestApp.Sagas
{
    /// <summary>
    /// Reacts to <see cref="TestListSagaTriggeredEvent"/> (dispatched asynchronously via the in-memory job
    /// queue) by renaming the list to <see cref="ReactionName"/>.
    /// </summary>
    [DomainClassId("0E1B8A3C-52D4-4C8E-8F6B-9A7C3D2E1F02")]
    public class TestListSaga(Guid id) : EventSourcedSaga(id)
    {
        public const string ReactionName = "renamed by saga";

        [SagaEvent(IsAlwaysStarting = true)]
        private void Handle(IEventMessage<TestListSagaTriggeredEvent> ev)
        {
            Send(new RenameTestListCommand(ev.Event.AggregateId, ReactionName));
        }
    }
}
