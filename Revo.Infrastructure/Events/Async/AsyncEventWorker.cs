using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Revo.Core.Core;
using Revo.Core.Events;

namespace Revo.Infrastructure.Events.Async
{
    public class AsyncEventWorker : IAsyncEventWorker
    {
        private readonly IAsyncEventQueueManager asyncEventQueueManager;
        private readonly IServiceLocator serviceLocator;
        private readonly ILogger logger;

        public AsyncEventWorker(IAsyncEventQueueManager asyncEventQueueManager,
            IServiceLocator serviceLocator, ILogger logger)
        {
            this.asyncEventQueueManager = asyncEventQueueManager;
            this.serviceLocator = serviceLocator;
            this.logger = logger;
        }

        public async Task RunQueueBacklogAsync(string queueName)
        {
            IAsyncEventQueueState queue = await asyncEventQueueManager.GetQueueStateAsync(queueName);
            if (queue == null)
            {
                return;
            }

            IReadOnlyCollection<IAsyncEventQueueRecord> records = await asyncEventQueueManager.GetQueueEventsAsync(queueName);

            if (records.Count == 0)
            {
                return;
            }

            bool processOnlyNonsequential = false;

            var nonSequential = records.Where(x => x.SequenceNumber == null).ToList();
            var sequential = records.Where(x => x.SequenceNumber != null).ToList();
            
            long? firstSequenceNumber = sequential.FirstOrDefault()?.SequenceNumber; //records should always arrive in order, but possibly with gaps in sequence
            long? lastSequenceNumber = sequential.LastOrDefault()?.SequenceNumber;

            logger.LogTrace($"Read {records.Count} async events from queue '{queueName}', last sequence number {lastSequenceNumber}");

            if (firstSequenceNumber != null
                && queue.LastSequenceNumberProcessed != null
                && (queue.LastSequenceNumberProcessed != firstSequenceNumber.Value - 1
                    || queue.LastSequenceNumberProcessed.Value + sequential.Count != lastSequenceNumber))
            {
                bool anyNonsequential = nonSequential.Any();
                if (anyNonsequential)
                {
                    logger.LogDebug($"Processing only non-sequential async events in '{queueName}' queue: missing some events in sequence at #{queue.LastSequenceNumberProcessed.Value + 1}");
                    processOnlyNonsequential = true;
                }
                else
                {
                    string error = $"Skipping processing of '{queueName}' async event queue: missing events in sequence at #{queue.LastSequenceNumberProcessed.Value + 1}";
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
            HashSet<IAsyncEventListener> usedListeners = new HashSet<IAsyncEventListener>();
            List<IAsyncEventQueueRecord> processedEvents = new List<IAsyncEventQueueRecord>();

            foreach (var record in records)
            {
                Type listenerType = typeof(IAsyncEventListener<>).MakeGenericType(record.EventMessage.Event.GetType());
                var handleAsyncMethod = listenerType.GetMethod(nameof(IAsyncEventListener<IEvent>.HandleAsync));
                
                IEnumerable<IAsyncEventListener> listeners = serviceLocator.GetAll(listenerType).Cast<IAsyncEventListener>();
                foreach (IAsyncEventListener listener in listeners)
                {
                    IAsyncEventSequencer sequencer = listener.EventSequencer;
                    IEnumerable<EventSequencing> sequences = sequencer.GetEventSequencing(record.EventMessage);
                    if (sequences.Any(x => x.SequenceName == queueName))
                    {
                        try
                        {
                            await (Task)handleAsyncMethod.Invoke(listener, new object[] { record.EventMessage, queueName });
                            usedListeners.Add(listener);
                        }
                        catch (Exception e)
                        {
                            string error = $"Failed processing of an async event {record.EventMessage.GetType().FullName} (ID: {record.EventId}) in queue '{queueName}' with listener {listener.GetType().FullName}";
                            logger.LogError(e, error);
                            throw new AsyncEventProcessingException(error, e);
                        }
                    }
                }

                processedEvents.Add(record);
            }

            foreach (IAsyncEventListener listener in usedListeners)
            {
                try
                {
                    await listener.OnFinishedEventQueueAsync(queueName);
                }
                catch (Exception e)
                {
                    string error = $"Failed to finish processing of an async event queue '{queueName}' with listener {listener.GetType().FullName}";
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
}
