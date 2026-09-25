using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Revo.EFCore.DataAccess.Entities;
using Xunit;

namespace Revo.EFCore.Tests.DataAccess.Entities
{
    public class RequestDbContextCacheTests
    {
        [Fact]
        public void Dispose_DisposesAddedContextsOnce()
        {
            var cache = new RequestDbContextCache();
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
            var cache = new RequestDbContextCache();
            var contexts = Enumerable.Range(0, 64).Select(_ => new ProbeContext()).ToArray();
            using var started = new Barrier(contexts.Length + 1);

            var tasks = contexts.Select(context => Task.Run(() =>
            {
                started.SignalAndWait();
                cache.AddDbContext(context);
            })).ToList();
            tasks.Add(Task.Run(() =>
            {
                started.SignalAndWait();
                cache.Dispose();
            }));

            await Task.WhenAll(tasks);
            cache.Dispose();

            contexts.Should().OnlyContain(x => x.DisposeCount == 1);
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
