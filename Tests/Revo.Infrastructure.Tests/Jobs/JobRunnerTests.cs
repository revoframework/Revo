using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using Revo.Core.Core;
using Revo.Infrastructure.Jobs;
using Xunit;

namespace Revo.Infrastructure.Tests.Jobs
{
    public class JobRunnerTests
    {
        private IServiceLocator serviceLocator;
        private JobRunner sut;

        public JobRunnerTests()
        {
            serviceLocator = Substitute.For<IServiceLocator>();
            sut = new JobRunner(serviceLocator);
        }
        
        [Fact]
        public async Task RunJobAsync()
        {
            var job = new TestJob();
            var jobHandler = Substitute.For<IJobHandler<TestJob>>();
            serviceLocator.Get(typeof(IJobHandler<TestJob>)).Returns(jobHandler);

            var cancellationToken = new CancellationToken();
            await sut.RunJobAsync(job, cancellationToken);

            jobHandler.Received(1).HandleAsync(job, cancellationToken);
        }

        [Fact]
        public async Task RunJobAsync_MultipleJobHandlersImplemented()
        {
            var job = new TestJob();
            var jobHandler = Substitute.ForPartsOf<MultiJobHandler>();
            serviceLocator.Get(typeof(IJobHandler<TestJob>)).Returns(jobHandler);

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