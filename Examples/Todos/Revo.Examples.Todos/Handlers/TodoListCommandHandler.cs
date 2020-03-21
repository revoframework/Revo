using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Revo.Core.Commands;
using Revo.Examples.Todos.Domain;
using Revo.Examples.Todos.Messages.Commands;
using Revo.Infrastructure.Repositories;

namespace Revo.Examples.Todos.Handlers
{
    public class TodoListCommandHandler :
        ICommandHandler<AddTodoCommand>,
        ICommandHandler<CreateTodoListCommand>,
        ICommandHandler<UpdateTodoListCommand>,
        ICommandHandler<UpdateTodoCommand>
    {
        private readonly IRepository repository;

        public TodoListCommandHandler(IRepository repository)
        {
            this.repository = repository;
        }

        public async Task HandleAsync(AddTodoCommand command, CancellationToken cancellationToken)
        {
            var todoList = await repository.GetAsync<TodoList>(command.TodoListId);
            todoList.AddTodo(command.Text);
        }

        public Task HandleAsync(CreateTodoListCommand command, CancellationToken cancellationToken)
        {
            var todoList = new TodoList(command.Id, command.Name);
            repository.Add(todoList);

            return Task.CompletedTask;
        }

        public async Task HandleAsync(UpdateTodoListCommand command, CancellationToken cancellationToken)
        {
            var todoList = await repository.GetAsync<TodoList>(command.Id);
            todoList.Rename(command.Name);
        }

        public async Task HandleAsync(UpdateTodoCommand command, CancellationToken cancellationToken)
        {
            var todoList = await repository.GetAsync<TodoList>(command.TodoListId);
            var todo = todoList.Todos.First(x => x.Id == command.TodoId);
            todo.UpdateText(command.Text);
            todo.MarkComplete(command.IsComplete);
        }
    }
}
