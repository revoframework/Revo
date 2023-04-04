using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Revo.Core.Core;
using Revo.Core.Events;
using Revo.Core.Services;

namespace Revo.Infrastructure.Events.Async;

public class AsyncEventWorker(
    IAsyncEventQueueManager asyncEventQueueManager,
    IServiceProvider serviceProvider,
    ILogger<AsyncEventWorker> logger)
    : IAsyncEventWorker
{
    public async Task RunQueueBacklogAsync(string queueName)
    {
        var queue = await asyncEventQueueManager.GetQueueStateAsync(queueName);
        if (queue == null)
        {
            return;
        }

        var records = await asyncEventQueueManager.GetQueueEventsAsync(queueName);

        if (records.Count == 0)
        {
            return;
        }

        var processOnlyNonsequential = false;

        var nonSequential = records.Where(x => x.SequenceNumber == null).ToList();
        var sequential = records.Where(x => x.SequenceNumber != null).ToList();
            
        var firstSequenceNumber = sequential.FirstOrDefault()?.SequenceNumber; //records should always arrive in order, but possibly with gaps in sequence
        var lastSequenceNumber = sequential.LastOrDefault()?.SequenceNumber;

        logger.LogTrace($"Read {records.Count} async events from queue '{queueName}', last sequence number {lastSequenceNumber}");

        if (firstSequenceNumber != null
            && queue.LastSequenceNumberProcessed != null
            && (queue.LastSequenceNumberProcessed != firstSequenceNumber.Value - 1
                || queue.LastSequenceNumberProcessed.Value + sequential.Count != lastSequenceNumber))
        {
            var anyNonsequential = nonSequential.Any();
            if (anyNonsequential)
            {
                logger.LogDebug($"Processing only non-sequential async events in '{queueName}' queue: missing some events in sequence at #{queue.LastSequenceNumberProcessed.Value + 1}");
                processOnlyNonsequential = true;
            }
            else
            {
                var error = $"Skipping processing of '{queueName}' async event queue: missing events in sequence at #{queue.LastSequenceNumberProcessed.Value + 1}";
                logger.LogWarning(error);
                throw new AsyncEventProcessingSequenceException(error, queue.LastSequenceNumberProcessed.Value + 1);
            }
        }

        if (nonSequential.Count > 0)
        {
            await ProcessEventsAsync(nonSequential, queueName);
        }

        if (!processOnlyNonsequential)
        {
            if (sequential.Count > 0)
            {
                await ProcessEventsAsync(sequential, queueName);
            }
        }

        if (processOnlyNonsequential)
        {
            throw new AsyncEventProcessingSequenceException(
                $"Processed only non-sequential async events in '{queueName}' queue: missing some events in sequence at #{queue.LastSequenceNumberProcessed.Value + 1}",
                queue.LastSequenceNumberProcessed.Value + 1);
        }
    }

    private async Task ProcessEventsAsync(IReadOnlyCollection<IAsyncEventQueueRecord> records, string queueName)
    {
        var usedListeners = new HashSet<IAsyncEventListener>();
        var processedEvents = new List<IAsyncEventQueueRecord>();

        foreach (var record in records)
        {
            var listenerType = typeof(IAsyncEventListener<>).MakeGenericType(record.EventMessage.Event.GetType());
            var handleAsyncMethod = listenerType.GetMethod(nameof(IAsyncEventListener<IEvent>.HandleAsync));
            
            var listeners = serviceProvider.GetServices(listenerType).Cast<IAsyncEventListener>();
            foreach (var listener in listeners)
            {
                var sequencer = listener.EventSequencer;
                var sequences = sequencer.GetEventSequencing(record.EventMessage);
                if (sequences.Any(x => x.SequenceName == queueName))
                {
                    try
                    {
                        await (Task)handleAsyncMethod.Invoke(listener, new object[] { record.EventMessage, queueName });
                        usedListeners.Add(listener);
                    }
                    catch (Exception e)
                    {
                        var error = $"Failed processing of an async event {record.EventMessage.GetType().FullName} (ID: {record.EventId}) in queue '{queueName}' with listener {listener.GetType().FullName}";
                        logger.LogError(e, error);
                        throw new AsyncEventProcessingException(error, e);
                    }
                }
            }

            processedEvents.Add(record);
        }

        foreach (var listener in usedListeners)
        {
            try
            {
                await listener.OnFinishedEventQueueAsync(queueName);
            }
            catch (Exception e)
            {
                var error = $"Failed to finish processing of an async event queue '{queueName}' with listener {listener.GetType().FullName}";
                logger.LogError(e, error);
                throw new AsyncEventProcessingException(error, e);
            }
        }

        foreach (var processedEvent in processedEvents)
        {
            await asyncEventQueueManager.DequeueEventAsync(processedEvent.Id);
        }

        await asyncEventQueueManager.CommitAsync();
    }
}