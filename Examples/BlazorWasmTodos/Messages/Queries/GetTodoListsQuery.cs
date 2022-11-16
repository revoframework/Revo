using Revo.Core.Commands;
using Revo.Examples.BlazorWasmTodos.Reads.Model;

namespace Revo.Examples.BlazorWasmTodos.Messages.Queries
{
    public class GetTodoListsQuery : IQuery<TodoListReadModel[]>
    {
    }
}
