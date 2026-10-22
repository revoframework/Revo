using System;
using Revo.Domain.Entities.Attributes;
using Revo.Domain.Entities.EventSourcing;
using Revo.IntegrationTests.TestApp.Domain.Events;

namespace Revo.IntegrationTests.TestApp.Domain
{
    [DomainClassId("6B0F2C1D-7A4E-4F43-9C59-2E8E1A7B5D01")]
    public class TestList : EventSourcedAggregateRoot
    {
        public TestList(Guid id, string name) : base(id)
        {
            Rename(name);
        }

        protected TestList(Guid id) : base(id)
        {
        }

        public string Name { get; private set; }

        public void Rename(string name)
        {
            if (Name != name)
            {
                Publish(new TestListRenamedEvent(name));
            }
        }

        public void TriggerSaga()
        {
            Publish(new TestListSagaTriggeredEvent());
        }

        private void Apply(TestListRenamedEvent ev)
        {
            Name = ev.Name;
        }
    }
}
