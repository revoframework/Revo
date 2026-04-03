using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Revo.Core.Lifecycle;
using Xunit;

namespace Revo.Core.Tests.Lifecycle;

public class ScopedHostedServiceTests
{
    private sealed class InvocationCounter
    {
        public int Count;
    }

    private sealed class CountingStartupTask(InvocationCounter counter) : IStartupTask
    {
        public Task ExecuteAsync(CancellationToken cancellationToken)
        {
            counter.Count++;
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task StartAsync_CreatesScopeAndRunsStartupTask()
    {
        var services = new ServiceCollection();
        services.AddSingleton<InvocationCounter>();
        services.AddScopedHostedService<CountingStartupTask>();

        await using var provider = services.BuildServiceProvider();
        var counter = provider.GetRequiredService<InvocationCounter>();
        var hosted = provider.GetServices<IHostedService>().OfType<ScopedHostedService<CountingStartupTask>>().Single();

        await hosted.StartAsync(CancellationToken.None);

        Assert.Equal(1, counter.Count);
    }

    [Fact]
    public async Task StartingAsync_OnLifecycleWrapper_CreatesScopeAndRunsStartupTask()
    {
        var services = new ServiceCollection();
        services.AddSingleton<InvocationCounter>();
        services.AddScopedHostedLifecycleStartingService<CountingStartupTask>();

        await using var provider = services.BuildServiceProvider();
        var counter = provider.GetRequiredService<InvocationCounter>();
        var hosted = provider.GetServices<IHostedService>()
            .OfType<ScopedHostedLifecycleStartingService<CountingStartupTask>>()
            .Single();

        await hosted.StartingAsync(CancellationToken.None);

        Assert.Equal(1, counter.Count);
    }
}
