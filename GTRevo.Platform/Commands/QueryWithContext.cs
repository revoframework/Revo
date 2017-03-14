using System;

namespace GTRevo.Platform.Commands
{
    public class QueryWithContext<T> : IQueryWithContext<T>
    {
        public Guid[] Context { get; set; }
    }
}
