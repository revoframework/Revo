using System;
using System.Collections.Generic;
using System.Text;

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
