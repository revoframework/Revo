using Revo.Domain.Events;

namespace Revo.IntegrationTests.TestApp.Domain.Events
{
    public class TestListRenamedEvent(string name) : DomainAggregateEvent
    {
        public string Name { get; } = name;
    }
}
