using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Revo.Core.Lifecycle;
using Revo.EFCore.DataAccess.Entities;
using Xunit;

namespace Revo.EFCore.Tests.DataAccess.Entities
{
    public class RequestDbContextCacheTests
    {
        [Fact]
        public void Dispose_DisposesAddedContextsOnce()
        {
            var cache = new RequestDbContextCache(new NullRequestLifetime());
            var contexts = new[] { new ProbeContext(), new ProbeContext() };

            foreach (var context in contexts)
            {
                cache.AddDbContext(context);
            }

            cache.Dispose();
            cache.Dispose();

            contexts.Should().OnlyContain(x => x.DisposeCount == 1);
        }

        [Fact]
        public async Task AddAndDispose_Concurrently_DisposesEveryContextOnce()
        {
            var cache = new RequestDbContextCache(new NullRequestLifetime());
            var contexts = Enumerable.Range(0, 64).Select(_ => new ProbeContext()).ToArray();
            using var started = new Barrier(contexts.Length + 1);

            var tasks = contexts.Select(context => RunOnDedicatedThread(() =>
            {
                started.SignalAndWait();
                cache.AddDbContext(context);
            })).ToList();
            tasks.Add(RunOnDedicatedThread(() =>
            {
                started.SignalAndWait();
                cache.Dispose();
            }));

            await Task.WhenAll(tasks);
            cache.Dispose();

            contexts.Should().OnlyContain(x => x.DisposeCount == 1);
        }

        /// <summary>
        /// The workers block on the barrier until all of them have started, so they must not run on thread pool
        /// threads - the pool would have to grow far beyond its minimum (= processor count), which it does only
        /// slowly (roughly one thread per second), making the test take tens of seconds.
        /// </summary>
        private static Task RunOnDedicatedThread(Action action)
        {
            return Task.Factory.StartNew(action, CancellationToken.None, TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
        }

        [Fact]
        public void AddDbContext_InRequest_LeavesDisposalToRequest()
        {
            var requestLifetime = Substitute.For<IRequestLifetime>();
            requestLifetime.TryRegisterForDispose(null).ReturnsForAnyArgs(true);
            var cache = new RequestDbContextCache(requestLifetime);
            var context = new ProbeContext();

            cache.AddDbContext(context);
            cache.Dispose();

            requestLifetime.Received(1).TryRegisterForDispose(context);
            context.DisposeCount.Should().Be(0);
        }

        private sealed class ProbeContext : DbContext
        {
            public ProbeContext()
                : base(new DbContextOptionsBuilder<ProbeContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options)
            {
            }

            public int DisposeCount { get; private set; }

            public override void Dispose()
            {
                DisposeCount++;
                base.Dispose();
            }
        }
    }
}
