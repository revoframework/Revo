using Revo.Core.Commands;
using Revo.DataAccess.Entities;
using Revo.Examples.Todos.Dto;
using Revo.Examples.Todos.Messages.Queries;
using Revo.Examples.Todos.Reads.Model;

namespace Revo.Examples.Todos.Reads.Handlers
{
    public class TaskListQueryHandler(IReadRepository readRepository) :
        IQueryHandler<GetTodoListsQuery, IQueryable<TodoListDto>>
    {
        public Task<IQueryable<TodoListDto>> HandleAsync(GetTodoListsQuery query, CancellationToken cancellationToken)
        {
            return Task.FromResult(readRepository
                .FindAll<TodoListReadModel>()
                .Select(x => new TodoListDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Todos = x.Todos.Select(t => new TodoDto()
                    {
                        Id = t.Id,
                        Text = t.Text,
                        IsComplete = t.IsComplete,
                        TodoListId = t.TodoListId // not x.Id - referencing the parent would require APPLY, unsupported by SQLite
                    }).ToList()
                }));
        }
    }
}
