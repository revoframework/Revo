using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Revo.Core.Core
{
    public static class TaskFactoryExtensions
    {
        /// <summary>
        /// Creates and starts a new Task whose execution is wrapped in a new task context.
        /// Task contexts are flown across the async calls using an AsyncLocal and can be
        /// used for fine-grained object lifetime scoping.
        /// </summary>
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

        /// <summary>
        /// Creates and starts a new Task whose execution is wrapped in a new task context.
        /// Task contexts are flown across the async calls using an AsyncLocal and can be
        /// used for fine-grained object lifetime scoping.
        /// </summary>
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

        /// <summary>
        /// Creates and starts a new Task whose execution is wrapped in a new task context.
        /// Task contexts are flown across the async calls using an AsyncLocal and can be
        /// used for fine-grained object lifetime scoping.
        /// </summary>
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

        /// <summary>
        /// Creates and starts a new Task whose execution is wrapped in a new task context.
        /// Task contexts are flown across the async calls using an AsyncLocal and can be
        /// used for fine-grained object lifetime scoping.
        /// </summary>
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
