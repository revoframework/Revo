using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Revo.EF6.DataAccess.Entities;
using Xunit;

namespace Revo.EF6.Tests.DataAccess.Entities
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

            Assert.All(contexts, context => Assert.Equal(1, context.DisposeCount));
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

            Assert.All(contexts, context => Assert.Equal(1, context.DisposeCount));
        }

        private sealed class ProbeContext : DbContext
        {
            static ProbeContext()
            {
                Database.SetInitializer<ProbeContext>(null);
            }

            public ProbeContext()
                : base("Data Source=request-db-context-cache-test")
            {
            }

            public int DisposeCount { get; private set; }

            protected override void Dispose(bool disposing)
            {
                DisposeCount++;
                base.Dispose(disposing);
            }
        }
    }
}
