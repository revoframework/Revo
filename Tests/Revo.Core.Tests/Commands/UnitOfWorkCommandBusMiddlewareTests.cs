using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Revo.Core.Commands;
using Revo.Core.Transactions;
using Xunit;

namespace Revo.Core.Tests.Commands
{
    public class UnitOfWorkCommandBusMiddlewareTests
    {
        private readonly UnitOfWorkCommandBusMiddleware sut;
        private readonly IUnitOfWorkFactory unitOfWorkFactory;
        private readonly CommandContextStack commandContextStack;
        private IUnitOfWork unitOfWork;

        public UnitOfWorkCommandBusMiddlewareTests()
        {
            unitOfWorkFactory = Substitute.For<IUnitOfWorkFactory>();
            unitOfWorkFactory.CreateUnitOfWork().Returns(ci =>
            {
                unitOfWork = Substitute.For<IUnitOfWork>();
                return unitOfWork;
            });

            commandContextStack = new CommandContextStack();

            sut = new UnitOfWorkCommandBusMiddleware(unitOfWorkFactory, commandContextStack);
        }
        
        [Fact]
        public async Task HandleAsync_PushesAndPopsCommandContext()
        {
            var command = new Command1();
            var expectedResult = new object();

            int invokedNext = 0;
            CommandBusMiddlewareDelegate next = async paramCommand =>
            {
                paramCommand.Should().Be(command);
                commandContextStack.CurrentCommand.Should().Be(command);
                unitOfWorkFactory.Received(1).CreateUnitOfWork();
                commandContextStack.UnitOfWork.Should().Be(unitOfWork);
                invokedNext++;
                return expectedResult;
            };

            var result = await sut.HandleAsync(command, CommandExecutionOptions.Default,
                next, CancellationToken.None);

            invokedNext.Should().Be(1);
            result.Should().Be(expectedResult);

            commandContextStack.PeekOrDefault.Should().BeNull();
        }
        
        [Fact]
        public async Task HandleAsync_ReturnsResult()
        {
            var query = new Query1();
            CommandBusMiddlewareDelegate next = async paramCommand => 42;

            var result = await sut.HandleAsync(query, CommandExecutionOptions.Default,
                next, CancellationToken.None);

            result.Should().Be(42);
        }

        [Fact]
        public async Task HandleAsync_CommitsForCommands()
        {
            var command = new Command1();
            CommandBusMiddlewareDelegate next = async paramCommand =>
            {
                commandContextStack.UnitOfWork.Should().Be(unitOfWork);
                return null;
            };

            await sut.HandleAsync(command, CommandExecutionOptions.Default,
                next, CancellationToken.None);

            unitOfWorkFactory.Received(1).CreateUnitOfWork();
            unitOfWork.Received(1).CommitAsync();
        }

        [Fact]
        public async Task HandleAsync_CommitsWhenConfigured()
        {
            var query = new Query1();
            CommandBusMiddlewareDelegate next = async paramCommand =>
            {
                commandContextStack.UnitOfWork.Should().Be(unitOfWork);
                return null;
            };

            await sut.HandleAsync(query, new CommandExecutionOptions(true, null),
                next, CancellationToken.None);

            unitOfWorkFactory.Received(1).CreateUnitOfWork();
            unitOfWork.Received(1).CommitAsync();
        }

        [Fact]
        public async Task HandleAsync_DoesNotCommitForQueries()
        {
            var query = new Query1();
            CommandBusMiddlewareDelegate next = async paramCommand =>
            {
                commandContextStack.UnitOfWork.Should().BeNull();
                return null;
            };

            await sut.HandleAsync(query, CommandExecutionOptions.Default,
                next, CancellationToken.None);

            unitOfWorkFactory.DidNotReceive().CreateUnitOfWork();
        }

        [Fact]
        public async Task HandleAsync_DoesNotCommitWhenConfigured()
        {
            var command = new Command1();
            CommandBusMiddlewareDelegate next = async paramCommand =>
            {
                commandContextStack.UnitOfWork.Should().BeNull();
                return null;
            };

            await sut.HandleAsync(command, new CommandExecutionOptions(false, null),
                next, CancellationToken.None);

            unitOfWorkFactory.DidNotReceive().CreateUnitOfWork();
        }

        [Fact]
        public async Task HandleAsync_PopsCommandContextOnException()
        {
            var command = new Command1();
            CommandBusMiddlewareDelegate next = async paramCommand => { throw new Exception(); };

            await sut.Awaiting(x => x.HandleAsync(command, CommandExecutionOptions.Default,
                next, CancellationToken.None)).Should().ThrowExactlyAsync<Exception>();

            commandContextStack.PeekOrDefault.Should().BeNull();
        }

        public class Command1 : ICommand
        {
        }

        public class Query1 : IQuery<int>
        {
        }
    }
}

