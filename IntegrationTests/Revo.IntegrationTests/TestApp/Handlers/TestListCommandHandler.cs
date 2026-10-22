using System;
using System.Threading;
using System.Threading.Tasks;
using Revo.Core.Commands;
using Revo.Infrastructure.Repositories;
using Revo.IntegrationTests.TestApp.Domain;
using Revo.IntegrationTests.TestApp.Messages.Commands;

namespace Revo.IntegrationTests.TestApp.Handlers
{
    public class TestListCommandHandler(IRepository repository, ICommandBus commandBus) :
        ICommandHandler<CreateTestListCommand>,
        ICommandHandler<RenameTestListCommand>,
        ICommandHandler<TriggerTestListSagaCommand>,
        ICommandHandler<CreateTestListWithChildrenCommand>,
        ICommandHandler<CreateTestListThenFailCommand>,
        ICommandHandler<AuthenticatedOnlyCommand>
    {
        public Task HandleAsync(CreateTestListCommand command, CancellationToken cancellationToken)
        {
            repository.Add(new TestList(command.Id, command.Name));
            return Task.CompletedTask;
        }

        public async Task HandleAsync(RenameTestListCommand command, CancellationToken cancellationToken)
        {
            var list = await repository.GetAsync<TestList>(command.Id);
            list.Rename(command.Name);
        }

        public async Task HandleAsync(TriggerTestListSagaCommand command, CancellationToken cancellationToken)
        {
            var list = await repository.GetAsync<TestList>(command.Id);
            list.TriggerSaga();
        }

        public async Task HandleAsync(CreateTestListWithChildrenCommand command, CancellationToken cancellationToken)
        {
            repository.Add(new TestList(command.Id, command.Name));

            foreach (var childId in command.ChildIds)
            {
                await commandBus.SendAsync(new CreateTestListCommand(childId, command.Name + " child"),
                    cancellationToken);
            }
        }

        public async Task HandleAsync(CreateTestListThenFailCommand command, CancellationToken cancellationToken)
        {
            repository.Add(new TestList(command.Id, "never saved"));
            await commandBus.SendAsync(new CreateTestListCommand(command.NestedId, "nested"), cancellationToken);
            throw new InvalidOperationException("Command failed after its nested command");
        }

        public Task HandleAsync(AuthenticatedOnlyCommand command, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
