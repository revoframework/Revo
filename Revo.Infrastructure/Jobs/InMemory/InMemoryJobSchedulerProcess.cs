using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Revo.Core.Core;

namespace Revo.Infrastructure.Jobs.InMemory;

public class InMemoryJobSchedulerProcess(IInMemoryJobWorkerProcess workerProcess)
    : IInMemoryJobSchedulerProcess, IHostedService
{
    private readonly BlockingCollection<ScheduledJob> unscheduledJobs = new();
    private readonly SortedList<DateTimeOffset, ScheduledJob> scheduledJobs = new();
    private Task workerTask;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        workerTask = Task.Run(() => Run(), cancellationToken);
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        unscheduledJobs.CompleteAdding();

        if (workerTask != null)
        {
            await workerTask.WaitAsync(cancellationToken);
        }
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