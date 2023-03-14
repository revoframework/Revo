using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Revo.Core.Core;
using Revo.Core.Lifecycle;

namespace Revo.Infrastructure.Jobs.InMemory
{
    public class InMemoryJobWorkerProcess : IApplicationStartedListener, IApplicationStoppingListener, IInMemoryJobWorkerProcess
    {
        private readonly BlockingCollection<EnqueuedJob> enqueuedJobs = new BlockingCollection<EnqueuedJob>();
        private readonly IJobRunner jobRunner;
        private readonly IInMemoryJobSchedulerConfiguration schedulerConfiguration;
        private readonly ILogger logger;

        private Task workerTask;

        public InMemoryJobWorkerProcess(IJobRunner jobRunner, IInMemoryJobSchedulerConfiguration schedulerConfiguration, ILogger logger)
        {
            this.jobRunner = jobRunner;
            this.schedulerConfiguration = schedulerConfiguration;
            this.logger = logger;
        }

        public void OnApplicationStarted()
        {
            workerTask = Task.Run(() => Run());
        }

        public void OnApplicationStopping()
        {
            enqueuedJobs.CompleteAdding();
            workerTask?.Wait();
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
                    task = Task.Factory.CreateNewWithContextWrapped(async () =>
                    {
                        try
                        {
                            await jobRunner.RunJobAsync(enqueuedJob.Job, CancellationToken.None);
                            runningTasks.TryRemove(task, out _);
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
}
