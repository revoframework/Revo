using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using System.Threading.Tasks;

namespace Revo.Core.Core
{
    public sealed class TaskContext : IDisposable
    {
        private static readonly AsyncLocal<TaskContext[]> CurrentLocal = new AsyncLocal<TaskContext[]>();
        private bool disposed = false;

        private TaskContext()
        {
        }

        public static TaskContext Current => CurrentLocal.Value?.LastOrDefault();

        public static TaskContext Enter()
        {
            var newContexts = CurrentLocal.Value;
            newContexts = newContexts != null ? newContexts.Append(new TaskContext()).ToArray() : new[] {new TaskContext()};

            CurrentLocal.Value = newContexts;
            return Current;
        }

        public void Dispose()
        {
            if (!disposed)
            {
                Debug.Assert(Current == this);

                CurrentLocal.Value = CurrentLocal.Value.Length == 1
                    ? null
                    : CurrentLocal.Value.Take(CurrentLocal.Value.Length - 1).ToArray();
            }
        }
    }
}
