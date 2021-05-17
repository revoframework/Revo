using System;
using System.Threading.Tasks;

namespace Revo.Core.Core
{
    public static class TaskFactoryExtensions
    {
        /// <summary>
        /// Creates a new Task whose execution is wrapped in a new task context.
        /// Task contexts are flown across the async calls using an AsyncLocal and can be
        /// used for fine-grained object lifetime scoping.
        /// </summary>
        public static Task CreateNewWithContext(this TaskFactory factory, Action action)
        {
            var task = new Task(() =>
            {
                using (TaskContext.Enter())
                {
                    action();
                }
            });
            
            return task;
        }

        /// <summary>
        /// Creates a new Task whose execution is wrapped in a new task context.
        /// Task contexts are flown across the async calls using an AsyncLocal and can be
        /// used for fine-grained object lifetime scoping.
        /// </summary>
        public static Task<TResult> CreateNewWithContext<TResult>(this TaskFactory factory, Func<TResult> function)
        {
            var task = new Task<TResult>(() =>
            {
                using (TaskContext.Enter())
                {
                    return function();
                }
            });
            
            return task;
        }

        /// <summary>
        /// Creates a new Task whose execution is wrapped in a new task context.
        /// Task contexts are flown across the async calls using an AsyncLocal and can be
        /// used for fine-grained object lifetime scoping.
        /// </summary>
        public static Task<Task> CreateNewWithContextWrapped(this TaskFactory factory, Func<Task> action)
        {
            var task = new Task<Task>(async () =>
            {
                using (TaskContext.Enter())
                {
                    await action();
                }
            });

            return task;
        }

        /// <summary>
        /// Creates a new Task whose execution is wrapped in a new task context.
        /// Task contexts are flown across the async calls using an AsyncLocal and can be
        /// used for fine-grained object lifetime scoping.
        /// </summary>
        public static Task<Task<TResult>> CreateNewWithContextWrapped<TResult>(this TaskFactory factory, Func<Task<TResult>> function)
        {
            var task = new Task<Task<TResult>>(async () =>
            {
                using (TaskContext.Enter())
                {
                    return await function();
                }
            });

            return task;
        }

        /// <summary>
        /// Creates and starts a new Task whose execution is wrapped in a new task context.
        /// Task contexts are flown across the async calls using an AsyncLocal and can be
        /// used for fine-grained object lifetime scoping.
        /// </summary>
        public static Task StartNewWithContext(this TaskFactory factory, Action action)
        {
            var task = CreateNewWithContext(factory, action);
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
            var task = CreateNewWithContext(factory, function);
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
            var task = CreateNewWithContextWrapped(factory, action);
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
            var task = CreateNewWithContextWrapped(factory, function);
            task.Start();

            return task.Unwrap();
        }
    }
}
