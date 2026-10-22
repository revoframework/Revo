using Revo.Core.Commands;
using Revo.Examples.Minimal.Messages.Commands;

namespace Revo.Examples.Minimal.Handlers;

public class TodoListCommandHandler :
    ICommandHandler<CreateTodoListCommand>
{
    public async Task HandleAsync(CreateTodoListCommand command, CancellationToken cancellationToken)
    {
        Console.WriteLine("CrateTodoListCommand handled");    
    }
}