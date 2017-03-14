using System;

namespace GTRevo.Platform.Core
{
    public interface IClock
    {
        DateTime Now { get; }
        DateTime UtcNow { get; }
    }
}