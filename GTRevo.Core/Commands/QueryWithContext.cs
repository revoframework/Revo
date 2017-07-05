using System;

namespace GTRevo.Core.Commands
{
    public class QueryWithContext<T> : IQueryWithContext<T>
    {
        public Guid[] Context { get; set; }
    }
}
