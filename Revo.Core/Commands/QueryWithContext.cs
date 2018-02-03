using System;

namespace Revo.Core.Commands
{
    public class QueryWithContext<T> : IQueryWithContext<T>
    {
        public Guid[] Context { get; set; }
    }
}
