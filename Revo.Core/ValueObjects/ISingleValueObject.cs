namespace Revo.Core.ValueObjects
{
    public interface ISingleValueObject
    {
        object Value { get; }
    }

    public interface ISingleValueObject<T> : ISingleValueObject
    {
    }
}
