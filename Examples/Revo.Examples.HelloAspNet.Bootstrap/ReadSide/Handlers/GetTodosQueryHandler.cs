using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Revo.Core.Commands;
using Revo.DataAccess.Entities;
using Revo.Examples.HelloAspNet.Bootstrap.Messages.Queries;
using Revo.Examples.HelloAspNet.Bootstrap.ReadSide.Model;

namespace Revo.Examples.HelloAspNet.Bootstrap.ReadSide.Handlers
{
    public class GetTodosQueryHandler : IQueryHandler<GetTodosQuery, IQueryable<TodoReadModel>>
    {
        private readonly IReadRepository readRepository;

        public GetTodosQueryHandler(IReadRepository readRepository)
        {
            this.readRepository = readRepository;
        }

        public Task<IQueryable<TodoReadModel>> HandleAsync(GetTodosQuery query, CancellationToken cancellationToken)
        {
            return Task.FromResult(
                readRepository.FindAll<TodoReadModel>());
        }
    }
}