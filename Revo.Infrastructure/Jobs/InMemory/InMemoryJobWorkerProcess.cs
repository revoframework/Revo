using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Revo.Infrastructure.Jobs.InMemory;

public class InMemoryJobWorkerProcess(
    IJobRunner jobRunner,
    IInMemoryJobSchedulerConfiguration schedulerConfiguration,
    ILogger<InMemoryJobWorkerProcess> logger)
    : IInMemoryJobWorkerProcess, IHostedService
{
    private readonly BlockingCollection<EnqueuedJob> enqueuedJobs = new();

    private Task workerTask;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        workerTask = Task.Run(() => Run(), cancellationToken);
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        enqueuedJobs.CompleteAdding();

        if (workerTask != null)
        {
            await workerTask.WaitAsync(cancellationToken);
        }
    }

    public void EnqueueJob(IJob job, Action<IJob, Exception> errorHandler)
    {
        try
        {
            enqueuedJobs.Add(new EnqueuedJob(job, errorHandler));
        }
        catch (InvalidOperationException)
        {
            logger.LogDebug($"Not enqueuing job(s) because scheduler worker process is already stopped");
        }
    }

    private void Run()
    {
        ConcurrentDictionary<Task, EnqueuedJob> runningTasks = new ConcurrentDictionary<Task, EnqueuedJob>();

        using (var throttle = new SemaphoreSlim(schedulerConfiguration.WorkerTaskParallelism))
        {
            while (!enqueuedJobs.IsCompleted)
            {
                EnqueuedJob enqueuedJob;
                try
                {
                    enqueuedJob = enqueuedJobs.Take();
                }
                catch (InvalidOperationException)
                {
                    continue;
                }

                throttle.Wait();
                Task jobTask = null;
                // created cold and only started after jobTask is tracked, so that the job can't finish
                // (and untrack itself) before it has even been added to runningTasks
                var task = new Task<Task>(async () =>
                {
                    try
                    {
                        // TODO pass CancellationToken
                        await jobRunner.RunJobAsync(enqueuedJob.Job, CancellationToken.None);
                    }
                    catch (Exception e)
                    {
                        enqueuedJob.ErrorHandler(enqueuedJob.Job, e);
                    }
                    finally
                    {
                        // release before untracking, so that the final WaitAll can't dispose the throttle before this
                        throttle.Release();
                        runningTasks.TryRemove(jobTask, out _);
                    }
                });

                jobTask = task.Unwrap();
                runningTasks[jobTask] = enqueuedJob;

                task.Start();
            }

            Task.WaitAll(runningTasks.Keys.ToArray());
        }
    }

    private class EnqueuedJob
    {
        public EnqueuedJob(IJob job, Action<IJob, Exception> errorHandler)
        {
            Job = job;
            ErrorHandler = errorHandler;
        }

        public IJob Job { get; }
        public Action<IJob, Exception> ErrorHandler { get; }
    }
}