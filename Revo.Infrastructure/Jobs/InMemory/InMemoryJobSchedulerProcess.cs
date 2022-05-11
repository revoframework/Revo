using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Revo.Core.Core;
using Revo.Core.Lifecycle;

namespace Revo.Infrastructure.Jobs.InMemory
{
    public class InMemoryJobSchedulerProcess : IApplicationStartedListener, IApplicationStoppingListener, IInMemoryJobSchedulerProcess
    {
        private readonly IInMemoryJobWorkerProcess workerProcess;
        private readonly BlockingCollection<ScheduledJob> unscheduledJobs = new BlockingCollection<ScheduledJob>();
        private readonly SortedList<DateTimeOffset, ScheduledJob> scheduledJobs = new SortedList<DateTimeOffset, ScheduledJob>();
        private Task workerTask;

        public InMemoryJobSchedulerProcess(IInMemoryJobWorkerProcess workerProcess)
        {
            this.workerProcess = workerProcess;
        }

        public void OnApplicationStarted()
        {
            workerTask = Task.Run(() => Run());
        }

        public void OnApplicationStopping()
        {
            unscheduledJobs.CompleteAdding();
            workerTask?.Wait();
        }

        public void ScheduleJob(IJob job, DateTimeOffset enqueueAt, Action<IJob, Exception> errorHandler)
        {
            unscheduledJobs.Add(new ScheduledJob(job, enqueueAt, errorHandler));
        }

        private void Run()
        {
            while (!unscheduledJobs.IsCompleted)
            {
                var now = Clock.Current.UtcNow;
                EnqueueJobs(now);

                ScheduledJob scheduledJob;
                try
                {
                    if (scheduledJobs.Count > 0)
                    {
                        var delta = scheduledJobs.Values[0].EnqueueAt - now;
                        delta = delta > TimeSpan.FromSeconds(30) ? TimeSpan.FromSeconds(30) : delta;
                        if (!unscheduledJobs.TryTake(out scheduledJob, delta))
                        {
                            continue;
                        }
                    }
                    else
                    {
                        scheduledJob = unscheduledJobs.Take();
                    }
                }
                catch (InvalidOperationException)
                {
                    continue;
                }

                scheduledJobs.Add(scheduledJob.EnqueueAt, scheduledJob);
                EnqueueJobs(now); // enqueue new jobs if needed
            }
        }

        private void EnqueueJobs(DateTimeOffset now)
        {
            var enqueued = new List<ScheduledJob>();
            for (int i = 0; i < scheduledJobs.Count && scheduledJobs.Values[i].EnqueueAt <= now; i++)
            {
                enqueued.Add(scheduledJobs.Values[i]);
                scheduledJobs.RemoveAt(i);
                i--;
            }

            enqueued.ForEach(x => workerProcess.EnqueueJob(x.Job, x.ErrorHandler));
        }

        private class ScheduledJob
        {
            public ScheduledJob(IJob job, DateTimeOffset enqueueAt, Action<IJob, Exception> errorHandler)
            {
                Job = job;
                EnqueueAt = enqueueAt;
                ErrorHandler = errorHandler;
            }

            public IJob Job { get; }
            public DateTimeOffset EnqueueAt { get; }
            public Action<IJob, Exception> ErrorHandler { get; }
        }
    }
}
