using System.Linq;
using Revo.Core.Commands;
using Revo.IntegrationTests.TestApp.Reads.Model;

namespace Revo.IntegrationTests.TestApp.Messages.Queries
{
    public class GetTestListsQuery : IQuery<IQueryable<TestListReadModel>>
    {
    }
}
