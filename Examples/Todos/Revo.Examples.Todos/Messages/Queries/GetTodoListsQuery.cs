using System.Linq;
using Revo.Core.Commands;
using Revo.Examples.Todos.Dto;

namespace Revo.Examples.Todos.Messages.Queries
{
    public class GetTodoListsQuery : IQuery<IQueryable<TodoListDto>>
    {
    }
}
