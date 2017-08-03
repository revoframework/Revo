using System;

namespace GTRevo.Infrastructure.Core.Domain.Basic
{
    public class BasicClassifier : BasicAggregateRoot, IClassifier
    {
        public BasicClassifier(Guid id, string code): base(id)
        {
            Code = code;
        }
        public string Code { get; private set; }
    }
}
