using System;
using System.Threading;
using System.Threading.Tasks;
using Revo.Core.Commands;
using Revo.Examples.HelloAspNet.Bootstrap.Domain;
using Revo.Examples.HelloAspNet.Bootstrap.Messages.Commands;
using Revo.Infrastructure.Repositories;

namespace Revo.Examples.HelloAspNet.Bootstrap.Handlers
{
    public class AddTodoCommandHandler : ICommandHandler<AddTodoCommand>
    {
        private readonly IRepository repository;

        public AddTodoCommandHandler(IRepository repository)
        {
            this.repository = repository;
        }

        public Task HandleAsync(AddTodoCommand command, CancellationToken cancellationToken)
        {
            Todo todo = new Todo(Guid.NewGuid(), command.Title);
            repository.Add(todo);
            return Task.FromResult(0);
        }
    }
}