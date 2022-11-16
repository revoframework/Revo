using Revo.Core.Commands;
using Revo.DataAccess.Entities;
using Revo.Examples.BlazorWasmTodos.Messages.Queries;
using Revo.Examples.BlazorWasmTodos.Reads.Model;

namespace Revo.Examples.BlazorWasmTodos.Reads.Handlers
{
    public class TaskListQueryHandler :
        IQueryHandler<GetTodoListsQuery, TodoListReadModel[]>
    {
        private readonly IReadRepository readRepository;

        public TaskListQueryHandler(IReadRepository readRepository)
        {
            this.readRepository = readRepository;
        }

        public async Task<TodoListReadModel[]> HandleAsync(GetTodoListsQuery query, CancellationToken cancellationToken)
        {
	        var taskLists = await readRepository.FindAll<TodoListReadModel>()
		        .Include(readRepository, x => x.Todos)
		        .ToArrayAsync(readRepository, cancellationToken);
	        return taskLists;
        }
    }
}
