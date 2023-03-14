using System.Diagnostics;
using System.Linq;
using System.Threading;
using Ninject.Infrastructure.Disposal;

namespace Revo.Core.Core
{
    public sealed class TaskContext : DisposableObject
    {
        private static readonly AsyncLocal<TaskContext[]> CurrentLocal = new AsyncLocal<TaskContext[]>();

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

        public override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                Debug.Assert(Current == this);

                CurrentLocal.Value = CurrentLocal.Value.Length == 1
                    ? null
                    : CurrentLocal.Value.Take(CurrentLocal.Value.Length - 1).ToArray();
            }

            base.Dispose(disposing);
        }
    }
}
