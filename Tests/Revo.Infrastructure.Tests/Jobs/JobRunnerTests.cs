using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Revo.Core.Services;
using Revo.Infrastructure.Jobs;
using Xunit;

namespace Revo.Infrastructure.Tests.Jobs
{
    public class JobRunnerTests
    {
        private IServiceProvider serviceProvider;
        private IServiceProvider scopedServiceProvider;
        private IVariantServiceResolver serviceResolver;
        private IServiceScope scope;
        private JobRunner sut;

        public JobRunnerTests()
        {
            serviceResolver = Substitute.For<IVariantServiceResolver>();
            scopedServiceProvider = Substitute.For<IServiceProvider>();
            scopedServiceProvider.GetService(typeof(IVariantServiceResolver)).Returns(serviceResolver);

            scope = Substitute.For<IServiceScope>();
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
            serviceResolver.GetService(typeof(IJobHandler<TestJob>)).Returns(jobHandler);

            var cancellationToken = new CancellationToken();
            await sut.RunJobAsync(job, cancellationToken);

            jobHandler.Received(1).HandleAsync(job, cancellationToken);
        }

        [Fact]
        public async Task RunJobAsync_MultipleJobHandlersImplemented()
        {
            var job = new TestJob();
            var jobHandler = Substitute.ForPartsOf<MultiJobHandler>();
            serviceResolver.GetService(typeof(IJobHandler<TestJob>)).Returns(jobHandler);

            var cancellationToken = new CancellationToken();
            await sut.RunJobAsync(job, cancellationToken);

            jobHandler.Received(1).HandleAsync(job, cancellationToken);
        }

        [Fact]
        public async Task RunJobAsync_DisposesScopeOnlyAfterJobFinishes()
        {
            var job = new TestJob();
            var jobHandler = Substitute.For<IJobHandler<TestJob>>();
            var jobCompletion = new TaskCompletionSource();
            jobHandler.HandleAsync(job, Arg.Any<CancellationToken>()).Returns(jobCompletion.Task);
            serviceResolver.GetService(typeof(IJobHandler<TestJob>)).Returns(jobHandler);

            var runTask = sut.RunJobAsync(job, CancellationToken.None);

            scope.DidNotReceive().Dispose();

            jobCompletion.SetResult();
            await runTask;

            scope.Received(1).Dispose();
        }

        [Fact]
        public async Task RunJobAsync_UsesHandlerOfBaseJobType()
        {
            var jobHandler = Substitute.For<IJobHandler<IBaseTestJob>>();
            var services = new ServiceCollection();
            services.AddSingleton(new VariantServiceTypeIndex(services));
            services.AddTransient<IVariantServiceResolver, VariantServiceResolver>();
            services.AddSingleton(jobHandler);

            var jobRunner = new JobRunner(services.BuildServiceProvider());
            var job = new DerivedTestJob();
            var cancellationToken = new CancellationToken();

            await jobRunner.RunJobAsync(job, cancellationToken);

            await jobHandler.Received(1).HandleAsync(job, cancellationToken);
        }

        public class TestJob : IJob
        { }

        public interface IBaseTestJob : IJob
        { }

        public class DerivedTestJob : IBaseTestJob
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