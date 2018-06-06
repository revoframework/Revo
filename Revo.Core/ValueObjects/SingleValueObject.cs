using System.Collections.Generic;
using Newtonsoft.Json;

namespace Revo.Core.ValueObjects
{
    [JsonConverter(typeof(SingleValueObjectJsonConverter))]
    public abstract class SingleValueObject<T, TValue> : ValueObject<T>, ISingleValueObject<TValue>
        where T : SingleValueObject<T, TValue>
    {
        public SingleValueObject(TValue value)
        {
            Value = value;
        }

        public TValue Value { get; private set; }
        object ISingleValueObject.Value => Value;

        protected override IEnumerable<(string Name, object Value)> GetValueComponents()
        {
            yield return (nameof(Value), Value);
        }
    }
}
