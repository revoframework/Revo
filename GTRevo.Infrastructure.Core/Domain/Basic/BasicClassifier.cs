using System;

namespace GTRevo.Infrastructure.Core.Domain.Basic
{
    public abstract class BasicClassifier : BasicAggregateRoot, IClassifier
    {
        protected BasicClassifier(Guid id, string code): base(id)
        {
            Code = code;
        }

        protected BasicClassifier()
        {
            
        }

        public string Code { get; private set; }
    }
}
