using System;

namespace Revo.Core.Core
{
    public interface IClock
    {
        DateTimeOffset Now { get; }
        DateTimeOffset UtcNow { get; }
    }
}