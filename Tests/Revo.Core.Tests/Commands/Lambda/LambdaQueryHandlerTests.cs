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
    public class LambdaQueryHandlerTests
    {
        private IServiceLocator serviceLocator;
        private IDummyService1 dummyService1;
        private IDummyService2 dummyService2;
        private LambdaQueryHandler sut;

        public LambdaQueryHandlerTests()
        {
            serviceLocator = Substitute.For<IServiceLocator>();
            sut = new LambdaQueryHandler(serviceLocator);

            dummyService1 = Substitute.For<IDummyService1>();
            serviceLocator.Get(typeof(IDummyService1)).Returns(dummyService1);

            dummyService2 = Substitute.For<IDummyService2>();
            serviceLocator.Get(typeof(IDummyService2)).Returns(dummyService2);
        }
        
        [Fact]
        public async Task SendLambda_Query()
        {
            var lambda = Substitute.For<Func<int>>();
            lambda.Invoke().Returns(1);
            var result = await sut.HandleAsync(new LambdaQuery(lambda, typeof(int)), CancellationToken.None);
            lambda.Received(1).Invoke();
            result.Should().Be(1);
        }

        [Fact]
        public async Task SendLambda_Query_Params1()
        {
            var lambda = Substitute.For<Func<IDummyService1, int>>();
            lambda.Invoke(dummyService1).Returns(1);
            var result = await sut.HandleAsync(new LambdaQuery(lambda, typeof(int)), CancellationToken.None);
            result.Should().Be(1);
        }

        [Fact]
        public async Task SendLambda_Query_Params2()
        {
            var lambda = Substitute.For<Func<IDummyService1, IDummyService2, int>>();
            lambda.Invoke(dummyService1, dummyService2).Returns(1);
            var result = await sut.HandleAsync(new LambdaQuery(lambda, typeof(int)), CancellationToken.None);
            result.Should().Be(1);
        }
        
        [Fact]
        public async Task SendLambda_Query_Task()
        {
            var lambda = Substitute.For<Func<Task<int>>>();
            lambda.Invoke().Returns(1);
            var result = await sut.HandleAsync(new LambdaQuery(lambda, typeof(int)), CancellationToken.None);
            lambda.Received(1).Invoke();
            result.Should().Be(1);
        }

        [Fact]
        public async Task SendLambda_Query_Task_Params1()
        {
            var lambda = Substitute.For<Func<IDummyService1, Task<int>>>();
            lambda.Invoke(dummyService1).Returns(1);
            var result = await sut.HandleAsync(new LambdaQuery(lambda, typeof(int)), CancellationToken.None);
            result.Should().Be(1);
        }

        [Fact]
        public async Task SendLambda_Query_Task_Params2()
        {
            var lambda = Substitute.For<Func<IDummyService1, IDummyService2, Task<int>>>();
            lambda.Invoke(dummyService1, dummyService2).Returns(1);
            var result = await sut.HandleAsync(new LambdaQuery(lambda, typeof(int)), CancellationToken.None);
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