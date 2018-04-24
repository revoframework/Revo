using System;
using System.Diagnostics;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using System.Threading.Tasks;

namespace Revo.Core.Core
{
    public sealed class TaskContext : IDisposable
    {
        private static readonly AsyncLocal<TaskContext> CurrentLocal = new AsyncLocal<TaskContext>();
        private bool disposed = false;

        private TaskContext()
        {
        }

        public static TaskContext Current => CurrentLocal.Value;

        public static TaskContext Enter()
        {
            if (Current != null)
            {
                throw new InvalidOperationException("A different TaskContext is already active.s");
            }

            CurrentLocal.Value = new TaskContext();
            return Current;
        }

        public void Dispose()
        {
            if (!disposed)
            {
                Debug.Assert(CurrentLocal.Value == this);
                CurrentLocal.Value = null;
            }
        }
    }
}
