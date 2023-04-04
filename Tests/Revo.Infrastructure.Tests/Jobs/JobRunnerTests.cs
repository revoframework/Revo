using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Revo.Infrastructure.Jobs;
using Xunit;

namespace Revo.Infrastructure.Tests.Jobs
{
    public class JobRunnerTests
    {
        private IServiceProvider serviceProvider;
        private IServiceProvider scopedServiceProvider;
        private JobRunner sut;

        public JobRunnerTests()
        {
            scopedServiceProvider = Substitute.For<IServiceProvider>();

            var scope = Substitute.For<IServiceScope>();
            scope.ServiceProvider.Returns(scopedServiceProvider);

            var serviceScopeFactory = Substitute.For<IServiceScopeFactory>();
            serviceScopeFactory.CreateScope().Returns(scope);

            serviceProvider = Substitute.For<IServiceProvider>();
            serviceProvider.GetService(typeof(IServiceScopeFactory)).Returns(serviceScopeFactory);

            sut = new JobRunner(serviceProvider);
        }
        
        [Fact]
        public async Task RunJobAsync()
        {
            var job = new TestJob();
            var jobHandler = Substitute.For<IJobHandler<TestJob>>();
            scopedServiceProvider.GetService(typeof(IJobHandler<TestJob>)).Returns(jobHandler);

            var cancellationToken = new CancellationToken();
            await sut.RunJobAsync(job, cancellationToken);

            jobHandler.Received(1).HandleAsync(job, cancellationToken);
        }

        [Fact]
        public async Task RunJobAsync_MultipleJobHandlersImplemented()
        {
            var job = new TestJob();
            var jobHandler = Substitute.ForPartsOf<MultiJobHandler>();
            scopedServiceProvider.GetService(typeof(IJobHandler<TestJob>)).Returns(jobHandler);

            var cancellationToken = new CancellationToken();
            await sut.RunJobAsync(job, cancellationToken);

            jobHandler.Received(1).HandleAsync(job, cancellationToken);
        }

        public class TestJob : IJob
        { }

        public class TestJob2 : IJob
        { }

        public class MultiJobHandler : IJobHandler<TestJob>, IJobHandler<TestJob2>
        {
            public virtual Task HandleAsync(TestJob job, CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
            }

            public virtual Task HandleAsync(TestJob2 job, CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
            }
        }
    }
}