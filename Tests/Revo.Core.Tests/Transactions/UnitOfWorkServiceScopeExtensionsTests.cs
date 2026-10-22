using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Revo.Core.Commands;
using Revo.Core.Transactions;
using Xunit;

namespace Revo.Core.Tests.Transactions
{
    public class UnitOfWorkServiceScopeExtensionsTests
    {
        private readonly IServiceScopeFactory serviceScopeFactory;
        private readonly IServiceScope scope;
        private readonly IServiceProvider scopeServiceProvider;
        private readonly IUnitOfWork unitOfWork;
        private readonly CommandContextStack commandContextStack = new();

        public UnitOfWorkServiceScopeExtensionsTests()
        {
            unitOfWork = Substitute.For<IUnitOfWork>();
            var unitOfWorkFactory = Substitute.For<IUnitOfWorkFactory>();
            unitOfWorkFactory.CreateUnitOfWork().Returns(unitOfWork);

            scopeServiceProvider = Substitute.For<IServiceProvider>();
            scopeServiceProvider.GetService(typeof(IUnitOfWorkFactory)).Returns(unitOfWorkFactory);
            scopeServiceProvider.GetService(typeof(CommandContextStack)).Returns(commandContextStack);

            scope = Substitute.For<IServiceScope>();
            scope.ServiceProvider.Returns(scopeServiceProvider);

            serviceScopeFactory = Substitute.For<IServiceScopeFactory>();
            serviceScopeFactory.CreateScope().Returns(scope);
        }

        [Fact]
        public async Task ExecuteInUnitOfWorkScopeAsync_RunsActionInScopeWithActiveUnitOfWork()
        {
            IServiceProvider actionServiceProvider = null;
            IUnitOfWork actionUnitOfWork = null;
            IUnitOfWork contextUnitOfWork = null;

            await serviceScopeFactory.ExecuteInUnitOfWorkScopeAsync((serviceProvider, uow) =>
            {
                actionServiceProvider = serviceProvider;
                actionUnitOfWork = uow;
                contextUnitOfWork = commandContextStack.UnitOfWork;
                unitOfWork.Received(1).Begin();
                unitOfWork.DidNotReceive().CommitAsync();
                scope.DidNotReceive().Dispose();
                return Task.CompletedTask;
            });

            actionServiceProvider.Should().BeSameAs(scopeServiceProvider);
            actionUnitOfWork.Should().BeSameAs(unitOfWork);
            contextUnitOfWork.Should().BeSameAs(unitOfWork);

            await unitOfWork.Received(1).CommitAsync();
            commandContextStack.PeekOrDefault.Should().BeNull();
            scope.Received(1).Dispose();
        }

        [Fact]
        public async Task ExecuteInUnitOfWorkScopeAsync_DoesNotCommitWhenActionFails()
        {
            var exception = new InvalidOperationException();

            var act = () => serviceScopeFactory.ExecuteInUnitOfWorkScopeAsync((_, _) => throw exception);

            (await act.Should().ThrowAsync<InvalidOperationException>()).Which.Should().BeSameAs(exception);
            await unitOfWork.DidNotReceive().CommitAsync();
            commandContextStack.PeekOrDefault.Should().BeNull();
            scope.Received(1).Dispose();
        }
    }
}
