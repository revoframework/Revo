using System;

namespace GTRevo.Core.Core
{
    public interface IClock
    {
        DateTime Now { get; }
        DateTime UtcNow { get; }
    }
}