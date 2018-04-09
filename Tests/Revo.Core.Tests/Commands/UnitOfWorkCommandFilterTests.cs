using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Revo.Core.Commands;
using Revo.Core.Transactions;
using Xunit;

namespace Revo.Core.Tests.Commands
{
    public class UnitOfWorkCommandFilterTests
    {
        private readonly UnitOfWorkCommandFilter sut;
        private readonly IUnitOfWorkFactory unitOfWorkFactory;
        private readonly CommandContextStack commandContextStack;
        private IUnitOfWork unitOfWork;

        public UnitOfWorkCommandFilterTests()
        {
            unitOfWorkFactory = Substitute.For<IUnitOfWorkFactory>();
            unitOfWorkFactory.CreateUnitOfWork().Returns(ci =>
            {
                unitOfWork = Substitute.For<IUnitOfWork>();
                return unitOfWork;
            });

            commandContextStack = new CommandContextStack();

            sut = new UnitOfWorkCommandFilter(unitOfWorkFactory, commandContextStack);
        }

        [Fact]
        public async Task PreFilterAsync_CreatesNewCommandContext()
        {
            var command = new Command1();
            await sut.PreFilterAsync(command);
            
            commandContextStack.CurrentCommand.Should().Be(command);
            unitOfWorkFactory.Received(1).CreateUnitOfWork();
            commandContextStack.UnitOfWork.Should().Be(unitOfWork);
        }

        [Fact]
        public async Task PostFilterAsync_Pops()
        {
            var command = new Command1();
            await sut.PreFilterAsync(command);
            await sut.PostFilterAsync(command, null);

            commandContextStack.PeekOrDefault.Should().BeNull();
        }

        [Fact]
        public async Task PostFilterAsync_CommitsForCommands()
        {
            var command = new Command1();
            await sut.PreFilterAsync(command);
            await sut.PostFilterAsync(command, null);

            unitOfWorkFactory.Received(1).CreateUnitOfWork();
            unitOfWork.Received(1).CommitAsync();
        }

        [Fact]
        public async Task PostFilterAsync_DoesNotCommitForQueries()
        {
            var query = new Query1();
            await sut.PreFilterAsync(query);
            await sut.PostFilterAsync(query, null);

            unitOfWorkFactory.Received(1).CreateUnitOfWork();
            unitOfWork.DidNotReceive().CommitAsync();
        }

        [Fact]
        public async Task FilterExceptionAsync_Pops()
        {
            var command = new Command1();
            await sut.PreFilterAsync(command);
            await sut.FilterExceptionAsync(command, new IOException());

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

