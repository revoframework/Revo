using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Revo.Core.Commands.Lambda;
using Revo.Core.Core;
using Xunit;

namespace Revo.Core.Tests.Commands.Lambda
{
    public class LambdaCommandHandlerTests
    {
        private IServiceLocator serviceLocator;
        private IDummyService1 dummyService1;
        private IDummyService2 dummyService2;
        private LambdaCommandHandler sut;

        public LambdaCommandHandlerTests()
        {
            serviceLocator = Substitute.For<IServiceLocator>();
            sut = new LambdaCommandHandler(serviceLocator);

            dummyService1 = Substitute.For<IDummyService1>();
            serviceLocator.Get(typeof(IDummyService1)).Returns(dummyService1);
            
            dummyService2 = Substitute.For<IDummyService2>();
            serviceLocator.Get(typeof(IDummyService2)).Returns(dummyService2);
        }

        [Fact]
        public async Task SendLambda_Command()
        {
            var lambda = Substitute.For<Action>();
            await sut.HandleAsync(new LambdaCommand(lambda), CancellationToken.None);

            lambda.Received(1).Invoke();
        }

        [Fact]
        public async Task SendLambda_Command_Params1()
        {
            var lambda = Substitute.For<Action<IDummyService1>>();
            await sut.HandleAsync(new LambdaCommand(lambda), CancellationToken.None);

            lambda.Received(1).Invoke(dummyService1);
        }

        [Fact]
        public async Task SendLambda_Command_Params2()
        {
            var lambda = Substitute.For<Action<IDummyService1, IDummyService2>>();
            await sut.HandleAsync(new LambdaCommand(lambda), CancellationToken.None);

            lambda.Received(1).Invoke(dummyService1, dummyService2);
        }

        [Fact]
        public async Task SendLambda_Query()
        {
            var lambda = Substitute.For<Func<int>>();
            lambda.Invoke().Returns(1);
            var result = await sut.HandleAsync(new LambdaResultCommand(lambda, typeof(int)), CancellationToken.None);
            lambda.Received(1).Invoke();
            result.Should().Be(1);
        }

        [Fact]
        public async Task SendLambda_ResultCommand_Params1()
        {
            var lambda = Substitute.For<Func<IDummyService1, int>>();
            lambda.Invoke(dummyService1).Returns(1);
            var result = await sut.HandleAsync(new LambdaResultCommand(lambda, typeof(int)), CancellationToken.None);
            result.Should().Be(1);
        }

        [Fact]
        public async Task SendLambda_ResultCommand_Params2()
        {
            var lambda = Substitute.For<Func<IDummyService1, IDummyService2, int>>();
            lambda.Invoke(dummyService1, dummyService2).Returns(1);
            var result = await sut.HandleAsync(new LambdaResultCommand(lambda, typeof(int)), CancellationToken.None);
            result.Should().Be(1);
        }

        [Fact]
        public async Task SendLambda_ResultCommand_Task()
        {
            var lambda = Substitute.For<Func<Task<int>>>();
            lambda.Invoke().Returns(1);
            var result = await sut.HandleAsync(new LambdaResultCommand(lambda, typeof(int)), CancellationToken.None);
            lambda.Received(1).Invoke();
            result.Should().Be(1);
        }

        [Fact]
        public async Task SendLambda_ResultCommand_Task_Params1()
        {
            var lambda = Substitute.For<Func<IDummyService1, Task<int>>>();
            lambda.Invoke(dummyService1).Returns(1);
            var result = await sut.HandleAsync(new LambdaResultCommand(lambda, typeof(int)), CancellationToken.None);
            result.Should().Be(1);
        }

        [Fact]
        public async Task SendLambda_ResultCommand_Task_Params2()
        {
            var lambda = Substitute.For<Func<IDummyService1, IDummyService2, Task<int>>>();
            lambda.Invoke(dummyService1, dummyService2).Returns(1);
            var result = await sut.HandleAsync(new LambdaResultCommand(lambda, typeof(int)), CancellationToken.None);
            result.Should().Be(1);
        }

        public interface IDummyService1
        {
        }

        public interface IDummyService2
        {
        }
    }
}