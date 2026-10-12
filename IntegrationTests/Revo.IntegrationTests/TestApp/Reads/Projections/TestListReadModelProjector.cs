using Revo.Core.Events;
using Revo.EFCore.DataAccess.Entities;
using Revo.EFCore.Projections;
using Revo.IntegrationTests.TestApp.Domain;
using Revo.IntegrationTests.TestApp.Domain.Events;
using Revo.IntegrationTests.TestApp.Reads.Model;

namespace Revo.IntegrationTests.TestApp.Reads.Projections
{
    public class TestListReadModelProjector(IEFCoreCrudRepository repository)
        : EFCoreEntityEventToPocoProjector<TestList, TestListReadModel>(repository)
    {
        private void Apply(IEventMessage<TestListRenamedEvent> ev)
        {
            Target.Name = ev.Event.Name;
        }
    }
}
