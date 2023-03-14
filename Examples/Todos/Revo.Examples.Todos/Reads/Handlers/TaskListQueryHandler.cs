using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Revo.Core.Commands;
using Revo.DataAccess.Entities;
using Revo.Examples.Todos.Dto;
using Revo.Examples.Todos.Messages.Queries;
using Revo.Examples.Todos.Reads.Model;

namespace Revo.Examples.Todos.Reads.Handlers
{
    public class TaskListQueryHandler :
        IQueryHandler<GetTodoListsQuery, IQueryable<TodoListDto>>
    {
        private readonly IReadRepository readRepository;
        private readonly IMapper mapper;

        public TaskListQueryHandler(IReadRepository readRepository, IMapper mapper)
        {
            this.readRepository = readRepository;
            this.mapper = mapper;
        }

        public Task<IQueryable<TodoListDto>> HandleAsync(GetTodoListsQuery query, CancellationToken cancellationToken)
        {
            var taskLists = mapper.ProjectTo<TodoListDto>(
                readRepository
                    .FindAll<TodoListReadModel>());
            return Task.FromResult(taskLists);
        }
    }
}
