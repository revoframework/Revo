using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Revo.Infrastructure.Events.Async;

/// <summary>
/// Catches up the event sources and processes the backlogged async event queues upon startup.
/// </summary>
/// <remarks>
/// Runs in <see cref="IHostedLifecycleService.StartedAsync"/>, i.e. only after the framework's state needed for the
/// event processing (entity types, sagas, permissions...) has been initialized in <see cref="IHostedService.StartAsync"/>
/// of the other hosted services.
/// </remarks>
public class AsyncEventExecutionCatchUp(
    IAsyncEventPipelineConfiguration asyncEventPipelineConfiguration,
    IServiceScopeFactory serviceScopeFactory,
    ILogger<AsyncEventExecutionCatchUp> logger)
    : IHostedLifecycleService
{
    private Task task;

    public Task StartingAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public async Task StartedAsync(CancellationToken cancellationToken)
    {
        task = Task.Run(InitializeAsync);

        if (asyncEventPipelineConfiguration.WaitForEventCatchUpsUponStartup)
        {
            await task;
        }
    }

    public Task StoppingAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (task != null)
        {
            await task;
        }
    }

    private async Task InitializeAsync()
    {
        await CatchUpEventSourcesAsync();
        await RunBackloggedQueuesAsync();
    }

    private async Task CatchUpEventSourcesAsync()
    {
        using (var scope = serviceScopeFactory.CreateScope())
        {
            foreach (var eventSourceCatchUp in scope.ServiceProvider.GetServices<IEventSourceCatchUp>())
            {
                try
                {
                    await eventSourceCatchUp.CatchUpAsync();
                }
                catch (Exception e)
                {
                    // TODO reschedule?
                    logger.LogError(e, $"Failed to catch-up an event source {eventSourceCatchUp} during startup");
                }
            }
        }
    }

    private async Task RunBackloggedQueuesAsync()
    {
        IReadOnlyCollection<string> backloggedQueueNames;
        using (var managerScope = serviceScopeFactory.CreateScope())
        {
            var asyncEventQueueManager =
                managerScope.ServiceProvider.GetRequiredService<IAsyncEventQueueManager>();
            backloggedQueueNames = await asyncEventQueueManager.GetNonemptyQueueNamesAsync();
        }

        var throttledTasks = new List<Task>();

        using (var throttle = new SemaphoreSlim(asyncEventPipelineConfiguration.CatchUpProcessingParallelism))
        {
            foreach (string queueName in backloggedQueueNames)
            {
                await throttle.WaitAsync();

                throttledTasks.Add(Task.Run(async () =>
                {
                    using var scope = serviceScopeFactory.CreateScope();
                    try
                    {
                        var asyncEventQueueBacklogWorker =
                            scope.ServiceProvider.GetRequiredService<IAsyncEventWorker>();
                        await asyncEventQueueBacklogWorker.RunQueueBacklogAsync(queueName);
                    }
                    catch (AsyncEventProcessingException e)
                    {
                        // TODO reschedule?
                        logger.LogError(e,
                            $"AsyncEventProcessingException occurred while processing async event queue {queueName} during startup");
                    }
                    finally
                    {
                        throttle.Release();
                    }
                }));
            }

            await Task.WhenAll(throttledTasks);
        }
    }
}