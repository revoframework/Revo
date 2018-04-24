using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Revo.Core.Core
{
    public static class TaskFactoryExtensions
    {
        public static Task StartNewWithContext(this TaskFactory factory, Action action)
        {
            Task task = null;

            task = new Task(() =>
            {
                using (TaskContext.Enter())
                {
                    action();
                }
            });

            task.Start();

            return task;
        }

        public static Task<TResult> StartNewWithContext<TResult>(this TaskFactory factory, Func<TResult> function)
        {
            Task<TResult> task = null;

            task = new Task<TResult>(() =>
            {
                using (TaskContext.Enter())
                {
                    return function();
                }
            });

            task.Start();

            return task;
        }

        public static Task StartNewWithContext(this TaskFactory factory, Func<Task> action)
        {
            Task<Task> task = null;

            task = new Task<Task>(async () =>
            {
                using (TaskContext.Enter())
                {
                    await action();
                }
            });

            task.Start();

            return task.Unwrap();
        }

        public static Task<TResult> StartNewWithContext<TResult>(this TaskFactory factory, Func<Task<TResult>> function)
        {
            Task<Task<TResult>> task = null;

            task = new Task<Task<TResult>>(async () =>
            {
                using (TaskContext.Enter())
                {
                    return await function();
                }
            });

            task.Start();
            return task.Unwrap();
        }
    }
}
