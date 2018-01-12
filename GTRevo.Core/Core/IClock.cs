using System;

namespace GTRevo.Core.Core
{
    public interface IClock
    {
        DateTimeOffset Now { get; }
        DateTimeOffset UtcNow { get; }
    }
}