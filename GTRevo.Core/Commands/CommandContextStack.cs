using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Core.Transactions;

namespace GTRevo.Core.Commands
{
    public class CommandContextStack : ICommandContext
    {
        private readonly Stack<ICommandContext> contexts = new Stack<ICommandContext>();

        public ICommandBase CurrentCommand => contexts.Count > 0 ? contexts.Peek().CurrentCommand : null;
        public IUnitOfWork UnitOfWork => contexts.Count > 0 ? contexts.Peek().UnitOfWork : null;
        public ICommandContext PeekOrDefault => contexts.Count > 0 ? contexts.Peek() : null;

        public void Push(ICommandContext context)
        {
            contexts.Push(context);
        }

        public ICommandContext Pop()
        {
            return contexts.Pop();
        }
    }
}
