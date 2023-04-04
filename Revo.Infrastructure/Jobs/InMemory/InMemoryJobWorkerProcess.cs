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
                Task<Task> task = null;
                task = Task.Factory.StartNew(async () =>
                {
                    try
                    {
                        // TODO pass CancellationToken
                        await jobRunner.RunJobAsync(enqueuedJob.Job, CancellationToken.None);
                        runningTasks.TryRemove(task, out _); // TODO shouldnt this be in finally?
                    }
                    catch (Exception e)
                    {
                        enqueuedJob.ErrorHandler(enqueuedJob.Job, e);
                    }
                    finally
                    {
                        throttle.Release();
                    }
                });

                var unwrappedTask = task.Unwrap();
                runningTasks[unwrappedTask] = enqueuedJob;

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