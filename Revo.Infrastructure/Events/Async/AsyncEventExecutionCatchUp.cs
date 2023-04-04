using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Revo.Core.Core;
using Revo.Core.Services;

namespace Revo.Infrastructure.Events.Async;

public class AsyncEventExecutionCatchUp(
    IEnumerable<IEventSourceCatchUp> eventSourceCatchUps,
    IAsyncEventQueueManager asyncEventQueueManager,
    IAsyncEventPipelineConfiguration asyncEventPipelineConfiguration,
    IServiceScopeFactory serviceScopeFactory,
    ILogger<AsyncEventExecutionCatchUp> logger)
    : IHostedService
{
    private readonly IEventSourceCatchUp[] eventSourceCatchUpsArray = eventSourceCatchUps.ToArray();
    private Task task;
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        task = Task.Run(InitializeAsync);

        if (asyncEventPipelineConfiguration.WaitForEventCatchUpsUponStartup)
        {
            await task;
        }
    }

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
        foreach (var eventSourceCatchUp in eventSourceCatchUpsArray)
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
        
    private async Task RunBackloggedQueuesAsync()
    {
        var backloggedQueueNames = await asyncEventQueueManager.GetNonemptyQueueNamesAsync();
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