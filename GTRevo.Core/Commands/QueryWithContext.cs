using System;

namespace GTRevo.Commands
{
    public class QueryWithContext<T> : IQueryWithContext<T>
    {
        public Guid[] Context { get; set; }
    }
}
