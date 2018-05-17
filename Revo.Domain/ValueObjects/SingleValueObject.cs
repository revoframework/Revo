using System;
using System.Collections.Generic;
using System.Text;

namespace Revo.Domain.ValueObjects
{
    public abstract class SingleValueObject<T, TValue> : ValueObject<T>
        where T : SingleValueObject<T, TValue>
    {
        public SingleValueObject(TValue value)
        {
            Value = value;
        }

        public TValue Value { get; private set; }

        protected override IEnumerable<(string Name, object Value)> GetValueComponents()
        {
            yield return (nameof(Value), Value);
        }
    }
}
