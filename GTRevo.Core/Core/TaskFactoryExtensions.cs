using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GTRevo.Core.Core
{
    public static class TaskFactoryExtensions
    {
        public static Task StartNewWithContext(this TaskFactory factory, Action action)
        {
            Task task = null;

            task = new Task(() =>
            {
                Debug.Assert(TaskContext.Current == null);
                TaskContext.Current = new TaskContext(task);
                try
                {
                    action();
                }
                finally
                {
                    TaskContext.Current = null;
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
                Debug.Assert(TaskContext.Current == null);
                TaskContext.Current = new TaskContext(task);
                try
                {
                    return function();
                }
                finally
                {
                    TaskContext.Current = null;
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
                Debug.Assert(TaskContext.Current == null);
                TaskContext.Current = new TaskContext(task);
                try
                {
                    await action();
                }
                finally
                {
                    TaskContext.Current = null;
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
                Debug.Assert(TaskContext.Current == null);
                TaskContext.Current = new TaskContext(task);
                try
                {
                    return await function();
                }
                finally
                {
                    TaskContext.Current = null;
                }
            });

            task.Start();

            return task.Unwrap();
        }
    }
}
