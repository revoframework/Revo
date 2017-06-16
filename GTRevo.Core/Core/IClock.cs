using System;

namespace GTRevo.Core
{
    public interface IClock
    {
        DateTime Now { get; }
        DateTime UtcNow { get; }
    }
}