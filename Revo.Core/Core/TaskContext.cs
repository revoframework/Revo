using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks;

namespace Revo.Core.Core
{
    public sealed class TaskContext
    {
        private const string ContextKey = "{C67AAD63-703A-4EC5-B0D7-1E3A7D95B616}";

        public TaskContext(Task task)
        {
            this.Task = task;
        }

        public Task Task { get; private set; }

        public static TaskContext Current
        {
            get => (TaskContext)CallContext.LogicalGetData(ContextKey);
            internal set
            {
                if (value == null)
                {
                    CallContext.FreeNamedDataSlot(ContextKey);
                }
                else
                {
                    CallContext.LogicalSetData(ContextKey, value);
                }
            }
        }
    }
}
