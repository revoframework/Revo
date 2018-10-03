using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Revo.Core.Core;
using Revo.Core.Events;
using Revo.DataAccess.Entities;
using Revo.Infrastructure.Events.Async;
using Revo.Infrastructure.Jobs;
using MoreLinq;
using NSubstitute;
using Xunit;

namespace Revo.Infrastructure.Tests.Events.Async
{
    public class AsyncEventProcessorTests
    {
        private AsyncEventProcessor sut;
        private List<IAsyncEventWorker> asyncEventQueueBacklogWorkers = new List<IAsyncEventWorker>();
        private List<(IAsyncEventWorker, string)> processedQueues = new List<(IAsyncEventWorker, string)>();
        private IAsyncEventQueueManager asyncEventQueueManager;
        private IJobScheduler jobScheduler;
        private List<IAsyncEventQueueRecord> events;
        private List<(string queueName, Exception e)> queueExceptions;
        private AsyncEventPipelineConfiguration asyncEventPipelineConfiguration;

        public AsyncEventProcessorTests()
        {
            asyncEventQueueManager = Substitute.For<IAsyncEventQueueManager>();
            jobScheduler = Substitute.For<IJobScheduler>();

            asyncEventPipelineConfiguration = new AsyncEventPipelineConfiguration()
            {
                AsyncProcessAttemptCount = 3,
                SyncProcessAttemptCount = 3,
                AsyncRescheduleDelayAfterSyncProcessFailure = TimeSpan.FromMinutes(1),
                AsyncProcessRetryTimeout = TimeSpan.FromMilliseconds(500),
                AsyncProcessRetryTimeoutMultiplier = 6,
                SyncProcessRetryTimeout = TimeSpan.FromMilliseconds(600),
                SyncProcessRetryTimeoutMultiplier = 4
            };

            IAsyncEventWorker AsyncEventQueueBacklogWorkerFunc()
            {
                var worker = Substitute.For<IAsyncEventWorker>();

                worker.WhenForAnyArgs(x => x.RunQueueBacklogAsync(null)).Do(
                    ci =>
                    {
                        string queue = ci.ArgAt<string>(0);

                        lock (processedQueues)
                        {
                            processedQueues.Add((worker, queue));
                        }

                        lock (queueExceptions)
                        {
                            int exIndex = queueExceptions.FindIndex(x => x.queueName == queue);
                            if (exIndex != -1)
                            {
                                Exception ex = queueExceptions[exIndex].e;
                                queueExceptions.RemoveAt(exIndex);
                                throw ex;
                            }
                        }

                        lock (events)
                        {
                            events.Where(x => x.QueueName == queue).ToArray().ForEach(x => events.Remove(x));
                        }
                    });

                lock (asyncEventQueueBacklogWorkers)
                {
                    asyncEventQueueBacklogWorkers.Add(worker);
                }

                return worker;
            }

            sut = new AsyncEventProcessor(AsyncEventQueueBacklogWorkerFunc,
                asyncEventQueueManager, jobScheduler, asyncEventPipelineConfiguration);

            var sleep = Substitute.For<ISleep>();
            Sleep.SetSleep(() => sleep); // TODO use a thread var

            events = new List<IAsyncEventQueueRecord>()
            {
                new FakeAsyncEventQueueRecord()
                {
                    EventId = Guid.NewGuid(), EventMessage = new EventMessage<Event1>(new Event1(), new Dictionary<string, string>()),
                    Id = Guid.NewGuid(), QueueName = "Queue1", SequenceNumber = 1
                },
                new FakeAsyncEventQueueRecord()
                {
                    EventId = Guid.NewGuid(), EventMessage = new EventMessage<Event1>(new Event1(), new Dictionary<string, string>()),
                    Id = Guid.NewGuid(), QueueName = "Queue2", SequenceNumber = 2
                },
                new FakeAsyncEventQueueRecord()
                {
                    EventId = Guid.NewGuid(), EventMessage = new EventMessage<Event1>(new Event1(), new Dictionary<string, string>()),
                    Id = Guid.NewGuid(), QueueName = "Queue1", SequenceNumber = 3
                }
            };

            asyncEventQueueManager.FindQueuedEventsAsync(null).ReturnsForAnyArgs(ci =>
            {
                Guid[] eventsIds = ci.ArgAt<Guid[]>(0);
                lock (events)
                {
                    var result = events.Where(x => eventsIds.Contains(x.Id)).ToList();
                    return result;
                }
            });

            queueExceptions = new List<(string queueName, Exception e)>();
        }

        [Fact]
        public async Task ProcessSynchronously_RunsOkay()
        {
            await sut.ProcessSynchronously(events);

            asyncEventQueueBacklogWorkers.Should().HaveCount(2);
            processedQueues.Select(x => x.Item1).Should().BeEquivalentTo(asyncEventQueueBacklogWorkers, cfg => cfg.ComparingByValue<IAsyncEventWorker>());
            processedQueues.Select(x => x.Item2).Should().BeEquivalentTo("Queue1", "Queue2");
            Sleep.Current.DidNotReceiveWithAnyArgs().SleepAsync(TimeSpan.Zero);
        }

        [Fact]
        public async Task ProcessSynchronously_RetriesOptimisticConcurrencyException()
        {
            queueExceptions.Add(("Queue1", new OptimisticConcurrencyException()));

            await sut.ProcessSynchronously(events);

            asyncEventQueueBacklogWorkers.Should().HaveCount(3);
            processedQueues.Select(x => x.Item1).Should().BeEquivalentTo(asyncEventQueueBacklogWorkers, cfg => cfg.ComparingByValue<IAsyncEventWorker>());
            processedQueues.Select(x => x.Item2).Should().BeEquivalentTo("Queue1", "Queue1", "Queue2");
            var queue1Processors = processedQueues.Where(x => x.Item2 == "Queue1").Select(x => x.Item1).ToArray();
            var queue2Processors = processedQueues.Where(x => x.Item2 == "Queue2").Select(x => x.Item1).ToArray();

            Received.InOrder(() =>
            {
                queue1Processors[0].RunQueueBacklogAsync("Queue1");
                Sleep.Current.SleepAsync(asyncEventPipelineConfiguration.SyncProcessRetryTimeout);
                queue1Processors[1].RunQueueBacklogAsync("Queue1");
            });

            queue2Processors[0].Received(1).RunQueueBacklogAsync("Queue2");
        }

        [Fact]
        public async Task ProcessSynchronously_RetriesAsyncEventProcessingSequenceException()
        {
            queueExceptions.Add(("Queue1", new AsyncEventProcessingSequenceException(null)));

            await sut.ProcessSynchronously(events);

            asyncEventQueueBacklogWorkers.Should().HaveCount(3);
            processedQueues.Select(x => x.Item1).Should().BeEquivalentTo(asyncEventQueueBacklogWorkers, cfg => cfg.ComparingByValue<IAsyncEventWorker>());
            processedQueues.Select(x => x.Item2).Should().BeEquivalentTo("Queue1", "Queue1", "Queue2");
            var queue1Processors = processedQueues.Where(x => x.Item2 == "Queue1").Select(x => x.Item1).ToArray();
            var queue2Processors = processedQueues.Where(x => x.Item2 == "Queue2").Select(x => x.Item1).ToArray();

            Received.InOrder(() =>
            {
                queue1Processors[0].RunQueueBacklogAsync("Queue1");
                Sleep.Current.SleepAsync(asyncEventPipelineConfiguration.SyncProcessRetryTimeout);
                queue1Processors[1].RunQueueBacklogAsync("Queue1");
            });

            queue2Processors[0].Received(1).RunQueueBacklogAsync("Queue2");
        }
        
        [Fact]
        public async Task ProcessSynchronously_TwoRetries()
        {
            queueExceptions.Add(("Queue1", new OptimisticConcurrencyException()));
            queueExceptions.Add(("Queue1", new OptimisticConcurrencyException()));

            await sut.ProcessSynchronously(events);

            asyncEventQueueBacklogWorkers.Should().HaveCount(4);
            processedQueues.Select(x => x.Item1).Should().BeEquivalentTo(asyncEventQueueBacklogWorkers, cfg => cfg.ComparingByValue<IAsyncEventWorker>());
            processedQueues.Select(x => x.Item2).Should().BeEquivalentTo("Queue1", "Queue1", "Queue1", "Queue2");
            var queue1Processors = processedQueues.Where(x => x.Item2 == "Queue1").Select(x => x.Item1).ToArray();
            var queue2Processors = processedQueues.Where(x => x.Item2 == "Queue2").Select(x => x.Item1).ToArray();

            Received.InOrder(() =>
            {
                queue1Processors[0].RunQueueBacklogAsync("Queue1");
                Sleep.Current.SleepAsync(asyncEventPipelineConfiguration.SyncProcessRetryTimeout);
                queue1Processors[1].RunQueueBacklogAsync("Queue1");
                Sleep.Current.SleepAsync(new TimeSpan(asyncEventPipelineConfiguration.SyncProcessRetryTimeout.Ticks
                    * asyncEventPipelineConfiguration.SyncProcessRetryTimeoutMultiplier));
                queue1Processors[2].RunQueueBacklogAsync("Queue1");
            });

            queue2Processors[0].Received(1).RunQueueBacklogAsync("Queue2");
        }

        [Fact]
        public async Task ProcessSynchronously_ReschedulesAsAsyncAfterFailedRetries()
        {
            queueExceptions.Add(("Queue1", new OptimisticConcurrencyException()));
            queueExceptions.Add(("Queue1", new OptimisticConcurrencyException()));
            queueExceptions.Add(("Queue1", new OptimisticConcurrencyException()));

            await sut.ProcessSynchronously(events);

            asyncEventQueueBacklogWorkers.Should().HaveCount(4);
            processedQueues.Select(x => x.Item1).Should().BeEquivalentTo(asyncEventQueueBacklogWorkers, cfg => cfg.ComparingByValue<IAsyncEventWorker>());
            processedQueues.Select(x => x.Item2).Should().BeEquivalentTo("Queue1", "Queue1", "Queue1", "Queue2");
            var queue1Processors = processedQueues.Where(x => x.Item2 == "Queue1").Select(x => x.Item1).ToArray();
            var queue2Processors = processedQueues.Where(x => x.Item2 == "Queue2").Select(x => x.Item1).ToArray();

            Received.InOrder(() =>
            {
                queue1Processors[0].RunQueueBacklogAsync("Queue1");
                Sleep.Current.SleepAsync(asyncEventPipelineConfiguration.SyncProcessRetryTimeout);
                queue1Processors[1].RunQueueBacklogAsync("Queue1");
                Sleep.Current.SleepAsync(new TimeSpan(asyncEventPipelineConfiguration.SyncProcessRetryTimeout.Ticks
                    * asyncEventPipelineConfiguration.SyncProcessRetryTimeoutMultiplier));
                queue1Processors[2].RunQueueBacklogAsync("Queue1");
                jobScheduler.EnqeueJobAsync(Arg.Is<ProcessAsyncEventsJob>(x =>
                        x.AttemptsLeft == asyncEventPipelineConfiguration.AsyncProcessAttemptCount
                        && x.QueueName == "Queue1"
                        && x.RetryTimeout == asyncEventPipelineConfiguration.AsyncProcessRetryTimeout),
                    asyncEventPipelineConfiguration.AsyncRescheduleDelayAfterSyncProcessFailure);
            });

            Sleep.Current.Received(2).SleepAsync(Arg.Any<TimeSpan>());
            queue2Processors[0].Received(1).RunQueueBacklogAsync("Queue2");
        }

        [Fact]
        public async Task ProcessSynchronously_DoesntRetryIfEventsProcessed()
        {
            queueExceptions.Add(("Queue1", new OptimisticConcurrencyException()));
            var events2 = events.ToList();
            events.Clear();

            await sut.ProcessSynchronously(events2);

            Sleep.Current.DidNotReceiveWithAnyArgs().SleepAsync(TimeSpan.Zero);

            asyncEventQueueBacklogWorkers.Should().HaveCount(2);
            processedQueues.Select(x => x.Item1).Should().BeEquivalentTo(asyncEventQueueBacklogWorkers, cfg => cfg.ComparingByValue<IAsyncEventWorker>());
            processedQueues.Select(x => x.Item2).Should().BeEquivalentTo("Queue1", "Queue2");
            var queue1Processors = processedQueues.Where(x => x.Item2 == "Queue1").Select(x => x.Item1).ToArray();
            var queue2Processors = processedQueues.Where(x => x.Item2 == "Queue2").Select(x => x.Item1).ToArray();

            queue1Processors[0].Received(1).RunQueueBacklogAsync("Queue1");
            queue2Processors[0].Received(1).RunQueueBacklogAsync("Queue2");
        }

        [Fact]
        public async Task EnqueueForAsyncProcessing_EnqueuesJob()
        {
            List<(IJob, TimeSpan?)> jobs = new List<(IJob, TimeSpan?)>();

            jobScheduler.WhenForAnyArgs(x => x.EnqeueJobAsync(null, null))
                .Do(ci =>
                {
                    jobs.Add((ci.ArgAt<IJob>(0), ci.ArgAt<TimeSpan?>(1)));
                });

            await sut.EnqueueForAsyncProcessingAsync(events, TimeSpan.FromSeconds(42));

            jobScheduler.ReceivedWithAnyArgs(2).EnqeueJobAsync(null, null);

            jobs.Should().HaveCount(2);
            jobs.Should().Contain(x =>
                x.Item2 == TimeSpan.FromSeconds(42)
                && x.Item1 is ProcessAsyncEventsJob
                && ((ProcessAsyncEventsJob)x.Item1).AttemptsLeft == asyncEventPipelineConfiguration.AsyncProcessAttemptCount
                && ((ProcessAsyncEventsJob)x.Item1).QueueName == "Queue1"
                && ((ProcessAsyncEventsJob)x.Item1).RetryTimeout == asyncEventPipelineConfiguration.AsyncProcessRetryTimeout);
            jobs.Should().Contain(x =>
                x.Item2 == TimeSpan.FromSeconds(42)
                && x.Item1 is ProcessAsyncEventsJob
                && ((ProcessAsyncEventsJob)x.Item1).AttemptsLeft == asyncEventPipelineConfiguration.AsyncProcessAttemptCount
                && ((ProcessAsyncEventsJob)x.Item1).QueueName == "Queue2"
                && ((ProcessAsyncEventsJob)x.Item1).RetryTimeout == asyncEventPipelineConfiguration.AsyncProcessRetryTimeout);
        }

        public class Event1 : IEvent
        {    
        }
    }
}
