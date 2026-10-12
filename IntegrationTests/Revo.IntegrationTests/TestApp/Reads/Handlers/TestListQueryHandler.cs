using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Revo.Core.Commands;
using Revo.DataAccess.Entities;
using Revo.IntegrationTests.TestApp.Messages.Queries;
using Revo.IntegrationTests.TestApp.Reads.Model;

namespace Revo.IntegrationTests.TestApp.Reads.Handlers
{
    public class TestListQueryHandler(IReadRepository readRepository) :
        IQueryHandler<GetTestListsQuery, IQueryable<TestListReadModel>>
    {
        public Task<IQueryable<TestListReadModel>> HandleAsync(GetTestListsQuery query,
            CancellationToken cancellationToken)
        {
            // deliberately returned unevaluated - it gets enumerated only when serializing the HTTP response,
            // i.e. after the query's command scope has already been disposed
            return Task.FromResult(readRepository.FindAll<TestListReadModel>());
        }
    }
}
